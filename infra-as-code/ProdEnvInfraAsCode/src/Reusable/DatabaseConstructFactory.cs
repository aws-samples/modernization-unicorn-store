using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using System;
using static ProdEnvInfraAsCode.UnicornStoreDeploymentEnvStackProps;
using SecMan = Amazon.CDK.AWS.SecretsManager;


namespace ProdEnvInfraAsCode.Reusable
{
    public abstract class DatabaseConstructFactory
    {
        protected DatabaseConstructFactory(UnicornStoreDeploymentEnvStackProps settings)
        {
            this.Settings = settings;
        }

        protected UnicornStoreDeploymentEnvStackProps Settings { get; private set; }

        protected virtual bool IsClustered => this.Settings.RdsKind != RdsType.RegularRds;

        protected abstract IInstanceEngine DbInstanceEgnine { get; }

        protected virtual IClusterEngine DbClusterEgnine => this.IsClustered ? null : (IClusterEngine)this.DbInstanceEgnine;

        internal virtual string DbConnStrBuilderServerPropName => "Server";

        internal virtual string DBConnStrBuilderUserPropName => "Username";

        internal virtual string DBConnStrBuilderPasswordPropName => "Password";

        protected virtual string ExistingAuroraDbParameterGroupName
            => throw new NotImplementedException($"No known Aurora Parameter Group Name for \"{this.Settings.DbEngine}\"");

        protected virtual InstanceClass DefaultDatabaseInstanceClass => InstanceClass.BURSTABLE2;

        protected virtual InstanceSize MinimumDatabaseInstanceSize => InstanceSize.SMALL;

        private InstanceClass DatabaseInstanceClass =>
            this.Settings.DatabaseInstanceClass ?? this.DefaultDatabaseInstanceClass;

        private InstanceSize DatabaseInstanceSize =>
            this.Settings.DatabaseInstanceSize ?? this.MinimumDatabaseInstanceSize;

        private InstanceType InstanceType =>
            InstanceType.Of(this.DatabaseInstanceClass, this.DatabaseInstanceSize);


        internal DatabaseConstructOutput CreateDatabaseConstruct(Construct parent, Vpc vpc, SecretValue databasePasswordSecret)
        {
            return this.IsClustered ?
                CreateDbClusterConstruct(parent, vpc, databasePasswordSecret)
                : CreateDbInstanceConstruct(parent, vpc, databasePasswordSecret);
        }

        private DatabaseConstructOutput CreateDbClusterConstruct(Construct parent, Vpc vpc, SecretValue databasePasswordSecret)
        {
            var database = new DatabaseCluster(parent, $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                new DatabaseClusterProps
                {
                    Engine = this.DbClusterEgnine,
                    ClusterIdentifier = $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}-Cluster",
                    ParameterGroup = ParameterGroup.FromParameterGroupName(
                        parent, $"{this.Settings.ScopeName}DbParamGroup", this.ExistingAuroraDbParameterGroupName
                    ),
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    MasterUser = new Login
                    {
                        Username = this.Settings.DbUsername,
                        Password = databasePasswordSecret
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

            return new DatabaseConstructOutput
            {
                Connections = database.Connections,
                EndpointAddress = database.ClusterEndpoint.Hostname,
                Port = clusterPort
            };
        }

        private DatabaseConstructOutput CreateDbInstanceConstruct(Construct parent, Vpc vpc, SecretValue databasePasswordSecret)
        {
            var database = new DatabaseInstance(parent, $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                new DatabaseInstanceProps
                {
                    InstanceType = this.InstanceType,
                    Vpc = vpc,
                    VpcPlacement = new SubnetSelection
                    {
                        SubnetType = this.Settings.DbSubnetType
                    },

                    DeletionProtection = this.Settings.DotNetEnvironment != "Development",
                    InstanceIdentifier = $"{this.Settings.ScopeName}-Database-{this.Settings.DbEngine}",
                    Engine = this.DbInstanceEgnine,
                    MasterUsername = this.Settings.DbUsername,
                    MasterUserPassword = databasePasswordSecret,
                    RemovalPolicy = RemovalPolicy.DESTROY
                }
            );

            return new DatabaseConstructOutput
            {
                EndpointAddress = database.InstanceEndpoint.Hostname,
                Connections = database.Connections,
                Port = database.DbInstanceEndpointPort
            };
        }
    }
}
