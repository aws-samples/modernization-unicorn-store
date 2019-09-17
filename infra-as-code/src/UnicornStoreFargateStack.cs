using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.RDS;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;
using InfraAsCode.Reusable;

namespace InfraAsCode
{
    public class UnicornStoreFargateStack : Stack
    {
        public UnicornStoreFargateStack(Construct parent, string id, UnicornStoreFargateStackProps settings) : base(parent, id, settings)
        {
            var vpc = new Vpc(this, $"{settings.ScopeName}VPC", new VpcProps { MaxAzs = settings.MaxAzs });

            SecMan.SecretProps databasePasswordSecretDef = SdkExtensions.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DatabasePassword", passwordLength: 8);
            SecMan.Secret databasePasswordSecret = databasePasswordSecretDef.CreateSecretConstruct(this);

            DatabaseConstructInfo database = CreateDatabaseConstruct(settings, vpc, databasePasswordSecret);

            var ecsCluster = new Cluster(this, $"{settings.ScopeName}{settings.Infrastructure}Cluster",
                new ClusterProps
                {
                    Vpc = vpc,
                    ClusterName = $"{settings.ScopeName}{settings.Infrastructure}Cluster",
                }
            );

            // TODO: replace existing ECR with one created by the Stack
            var imageRepository = Repository.FromRepositoryName(this, "ExistingEcrRepository", settings.DockerImageRepository);

            var secService = new ApplicationLoadBalancedFargateService(this, $"{settings.ScopeName}FargateService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = ecsCluster,
                    DesiredCount = settings.DesiredReplicaCount,
                    Cpu = settings.CpuMillicores,
                    MemoryLimitMiB = settings.MemoryMiB,
                    Image = ContainerImage.FromEcrRepository(imageRepository, settings.ImageTag),
                    PublicLoadBalancer = settings.PublicLoadBalancer,
                    Environment = new Dictionary<string, string>()
                    {
                        { "ASPNETCORE_ENVIRONMENT", settings.DotNetEnvironment ?? "Production" },
                        { "DefaultAdminUsername", settings.DefaultSiteAdminUsername },
                        { $"UnicornDbConnectionStringBuilder__{settings.DbConnStrBuilderServerPropName}",
                            database.EndpointAddress },
                        { $"UnicornDbConnectionStringBuilder__{settings.DBConnStrBuilderUserPropName}",
                            settings.DbUsername }, 
                    },
                    Secrets = new Dictionary<string, Secret>
                    {
                        { "DefaultAdminPassword", SdkExtensions.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DefaultSiteAdminPassword").CreateSecret(this) },
                        { $"UnicornDbConnectionStringBuilder__{settings.DBConnStrBuilderPasswordPropName}",
                            databasePasswordSecret.CreateSecret(this, databasePasswordSecretDef.SecretName) }
                    }
                }
            );

            // Update RDS Security Group to allow inbound database connections from the Fargate Service Security Group
            database.Connections.AllowDefaultPortFrom(secService.Service.Connections.SecurityGroups[0]);
        }

        private DatabaseConstructInfo CreateDatabaseConstruct(UnicornStoreFargateStackProps settings, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            if (settings.DbEngine == UnicornStoreFargateStackProps.DbEngineType.SQLSERVER
                || settings.RdsKind == UnicornStoreFargateStackProps.RdsType.RegularRds)
            {
                return CreateDbInstanceConstruct(settings, vpc, databasePasswordSecret);
            }

            return CreateDbClusterConstruct(settings, vpc, databasePasswordSecret);
        }

        private DatabaseConstructInfo CreateDbClusterConstruct(UnicornStoreFargateStackProps settings, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            var database = new DatabaseCluster(this, $"{settings.ScopeName}-Database-{settings.DbEngine}",
                new DatabaseClusterProps
                {
                    Engine = settings.DbClusterEgnine,
                    ClusterIdentifier = $"{settings.ScopeName}-Database-{settings.DbEngine}",
                    
                    //ParameterGroup = new ParameterGroup(this, $"{settings.ScopeName}ParamGroup",
                    //    new ParameterGroupProps
                    //    {
                    //        Family = "aurora-mysql5.7"
                    //    }),
                    MasterUser = new Login
                    {
                        Username = settings.DbUsername,
                        Password = databasePasswordSecret.SecretValue
                    },
                    InstanceProps = new Amazon.CDK.AWS.RDS.InstanceProps
                    {
                        InstanceType = InstanceType.Of(settings.DatabaseInstanceClass, settings.DatabaseInstanceSize),
                        Vpc = vpc,
                        VpcSubnets = new SubnetSelection
                        {
                            SubnetType = settings.DbSubnetType
                        },
                        
                    }
                }
            );

            return new DatabaseConstructInfo
            {
                Connections = database.Connections,
                EndpointAddress = database.ClusterEndpoint.Hostname
            };
        }

        private DatabaseConstructInfo CreateDbInstanceConstruct(UnicornStoreFargateStackProps settings, Vpc vpc, SecMan.Secret databasePasswordSecret)
        {
            var database = new DatabaseInstance(this, $"{settings.ScopeName}-Database-{settings.DbEngine}",
                new DatabaseInstanceProps
                {
                    InstanceClass = InstanceType.Of(settings.DatabaseInstanceClass, settings.DatabaseInstanceSize),
                    Vpc = vpc,
                    DeletionProtection = settings.DotNetEnvironment != "Development",
                    VpcPlacement = new SubnetSelection
                    {
                        SubnetType = settings.DbSubnetType
                    },

                    InstanceIdentifier = $"{settings.ScopeName}-Database-{settings.DbEngine}",
                    Engine = settings.DbInstanceEgnine,
                    //DatabaseName = $"{stackProps.ScopeName}", // Can't be specified, at least not for SQL Server
                    MasterUsername = settings.DbUsername,
                    MasterUserPassword = databasePasswordSecret.SecretValue,
                }
            );

            return new DatabaseConstructInfo
            {
                EndpointAddress = database.DbInstanceEndpointAddress,
                Connections = database.Connections
            };
        }
    }
}
