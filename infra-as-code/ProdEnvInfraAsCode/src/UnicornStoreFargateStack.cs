using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;
using ProdEnvInfraAsCode.Reusable;

namespace ProdEnvInfraAsCode
{
    public class UnicornStoreFargateStack : Stack
    {
        public UnicornStoreFargateStack(Construct parent, string id, UnicornStoreProdEnvStackProps settings) : base(parent, id, settings)
        {
            var vpc = new Vpc(this, $"{settings.ScopeName}VPC", new VpcProps { MaxAzs = settings.MaxAzs });

            SecMan.SecretProps databasePasswordSecretDef = 
                SdkExtensions.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DatabasePassword", passwordLength: 8);
            SecMan.Secret databasePasswordSecret = databasePasswordSecretDef.CreateSecretConstruct(this);

            var dbConstructFactory = settings.CreateDbConstructFactory();

            DatabaseConstructInfo database =
                dbConstructFactory.CreateDatabaseConstruct(this, vpc, databasePasswordSecret);

            var ecsCluster = new Cluster(this, $"{settings.ScopeName}{settings.Infrastructure}Cluster",
                new ClusterProps
                {
                    Vpc = vpc,
                    ClusterName = $"{settings.ScopeName}{settings.Infrastructure}Cluster",
                }
            );

            // TODO: replace existing ECR with one created by the Stack
            var imageRepository = Repository.FromRepositoryName(this, "ExistingEcrRepository", settings.DockerImageRepository);

            var ecsService = new ApplicationLoadBalancedFargateService(this, $"{settings.ScopeName}FargateService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = ecsCluster,
                    DesiredCount = settings.DesiredComputeReplicaCount,
                    Cpu = settings.CpuMillicores,
                    MemoryLimitMiB = settings.MemoryMiB,
                    Image = ContainerImage.FromEcrRepository(imageRepository, settings.ImageTag),
                    PublicLoadBalancer = settings.PublicLoadBalancer,
                    Environment = new Dictionary<string, string>()
                    {
                        { "ASPNETCORE_ENVIRONMENT", settings.DotNetEnvironment ?? "Production" },
                        { "DefaultAdminUsername", settings.DefaultSiteAdminUsername },
                        { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DbConnStrBuilderServerPropName}",
                            database.EndpointAddress },
                        { $"UnicornDbConnectionStringBuilder__Port", database.Port },
                        { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DBConnStrBuilderUserPropName}",
                            settings.DbUsername }, 
                    },
                    Secrets = new Dictionary<string, Secret>
                    {
                        { "DefaultAdminPassword", SdkExtensions.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DefaultSiteAdminPassword").CreateSecret(this) },
                        { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DBConnStrBuilderPasswordPropName}",
                            databasePasswordSecret.CreateSecret(this, databasePasswordSecretDef.SecretName) }
                    }
                }
            );

            // Update RDS Security Group to allow inbound database connections from the Fargate Service Security Group
            database.Connections.AllowDefaultPortFrom(ecsService.Service.Connections.SecurityGroups[0]);
        }
    }
}
