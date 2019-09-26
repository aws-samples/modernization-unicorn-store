using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using System;
using static ProdEnvInfraAsCode.UnicornStoreProdEnvStackProps;
using SecMan = Amazon.CDK.AWS.SecretsManager;


namespace ProdEnvInfraAsCode.Reusable
{
    public abstract class DatabaseConstructFactory
    {
        protected DatabaseConstructFactory(UnicornStoreProdEnvStackProps settings)
        {
            this.Settings = settings;
        }

        protected UnicornStoreProdEnvStackProps Settings { get; private set; }

        protected virtual bool IsClustered => this.Settings.RdsKind != RdsType.RegularRds;

        protected abstract DatabaseInstanceEngine DbInstanceEgnine { get; }

        protected virtual DatabaseClusterEngine DbClusterEgnine => this.IsClustered ? null : this.DbInstanceEgnine;

        internal virtual string DbConnStrBuilderServerPropName => "Server";

        internal virtual string DBConnStrBuilderUserPropName => "Username";

        internal virtual string DBConnStrBuilderPasswordPropName => "Password";

        protected virtual string ExistingAuroraDbParameterGroupName
            => throw new NotImplementedException($"No known Aurora Parameter Group Name for \"{this.Settings.DbEngine}\"");

        protected virtual InstanceClass DefaultDatabaseInstanceClass => InstanceClass.BURSTABLE2;

        protected virtual InstanceSize MinimuDatabaseInstanceSize => InstanceSize.SMALL;

        private InstanceClass DatabaseInstanceClass =>
            this.Settings.DatabaseInstanceClass ?? this.DefaultDatabaseInstanceClass;

        private InstanceSize DatabaseInstanceSize =>
            this.Settings.DatabaseInstanceSize ?? this.MinimuDatabaseInstanceSize;

        private InstanceType InstanceType =>
            InstanceType.Of(this.DatabaseInstanceClass, this.DatabaseInstanceSize);


        internal DatabaseConstructInfo CreateDatabaseConstruct(Construct parent, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            return this.IsClustered ?
                CreateDbClusterConstruct(parent, vpc, databasePasswordSecret)
                : CreateDbInstanceConstruct(parent, vpc, databasePasswordSecret);
        }

        private DatabaseConstructInfo CreateDbClusterConstruct(Construct parent, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            var database = new DatabaseCluster(parent, $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                new DatabaseClusterProps
                {
                    Engine = this.DbClusterEgnine,
                    ClusterIdentifier = $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}-Cluster",
                    ParameterGroup = ClusterParameterGroup.FromParameterGroupName(
                        parent, $"{this.Settings.ScopeName}DbParamGroup", this.ExistingAuroraDbParameterGroupName
                    ),
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    MasterUser = new Login
                    {
                        Username = this.Settings.DbUsername,
                        Password = databasePasswordSecret.SecretValue
                    },
                    InstanceProps = new Amazon.CDK.AWS.RDS.InstanceProps
                    {
                        InstanceType = this.InstanceType,
                        Vpc = vpc,
                        VpcSubnets = new SubnetSelection
                        {
                            SubnetType = this.Settings.DbSubnetType
                        }
                    }
                }
            );

            string clusterPort = database.ClusterEndpoint.SocketAddress.Split(':')[1]; // Bad - port placeholder is not available otherwise

            return new DatabaseConstructInfo
            {
                Connections = database.Connections,
                EndpointAddress = database.ClusterEndpoint.Hostname,
                Port = clusterPort
            };
        }

        private DatabaseConstructInfo CreateDbInstanceConstruct(Construct parent, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            var database = new DatabaseInstance(parent, $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                new DatabaseInstanceProps
                {
                    InstanceClass = this.InstanceType,
                    Vpc = vpc,
                    VpcPlacement = new SubnetSelection
                    {
                        SubnetType = this.Settings.DbSubnetType
                    },

                    DeletionProtection = this.Settings.DotNetEnvironment != "Development",
                    InstanceIdentifier = $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                    Engine = this.DbInstanceEgnine,
                    MasterUsername = this.Settings.DbUsername,
                    MasterUserPassword = databasePasswordSecret.SecretValue,
                    RemovalPolicy = RemovalPolicy.DESTROY
                }
            );

            return new DatabaseConstructInfo
            {
                EndpointAddress = database.InstanceEndpoint.Hostname,
                Connections = database.Connections,
                Port = database.DbInstanceEndpointPort
            };
        }
    }
}
