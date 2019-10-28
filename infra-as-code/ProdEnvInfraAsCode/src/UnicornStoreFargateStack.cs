using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;
using ProdEnvInfraAsCode.Reusable;
using CdkLib;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;

namespace ProdEnvInfraAsCode
{
    public class UnicornStoreFargateStack : Stack
    {
        protected readonly UnicornStoreDeploymentEnvStackProps settings;

        public UnicornStoreFargateStack(Construct parent, string id, UnicornStoreDeploymentEnvStackProps settings) : base(parent, id, settings)
        {
            this.settings = settings;

            var vpc = new Vpc(this, $"{settings.ScopeName}VPC", new VpcProps { MaxAzs = settings.MaxAzs });

            SecMan.SecretProps databasePasswordSecretSettings =
                Helpers.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DatabasePassword", passwordLength: 8);
            SecMan.Secret databasePasswordSecretConstruct = databasePasswordSecretSettings.CreateSecretConstruct(this);

            var dbConstructFactory = settings.CreateDbConstructFactory();

            DatabaseConstructOutput dbConstructOutput =
                dbConstructFactory.CreateDatabaseConstruct(this, vpc, databasePasswordSecretConstruct.SecretValue);

            var ecsCluster = new Cluster(this, $"Application{settings.Infrastructure}Cluster", new ClusterProps
                {
                    Vpc = vpc,
                    ClusterName = settings.EcsClusterName
                }
            );

            ApplicationLoadBalancedFargateService ecsService = this.CreateEcsService(
                ecsCluster,
                databasePasswordSecretConstruct.CreateSecret(this, databasePasswordSecretSettings.SecretName), 
                dbConstructFactory, 
                dbConstructOutput
            );

            // Update RDS Security Group to allow inbound database connections from the Fargate Service Security Group
            dbConstructOutput.Connections.AllowDefaultPortFrom(ecsService.Service.Connections.SecurityGroups[0]);
        }

        private ApplicationLoadBalancedFargateService CreateEcsService(
                    Cluster ecsCluster,
                    Secret dbPasswordSecret,
                    DatabaseConstructFactory dbConstructFactory, 
                    DatabaseConstructOutput dbConstructOutput 
                    )
        {
            var imageRepository = Repository.FromRepositoryName(this, "ExistingEcrRepository", settings.DockerImageRepository);

            var ecsService = new ApplicationLoadBalancedFargateService(this, $"{settings.ScopeName}FargateService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = ecsCluster,
                    DesiredCount = settings.DesiredComputeReplicaCount,
                    Cpu = settings.CpuMillicores,
                    MemoryLimitMiB = settings.MemoryMiB,
                    PublicLoadBalancer = settings.PublicLoadBalancer,
                    LoadBalancer = new ApplicationLoadBalancer(this, $"{settings.ScopeName}-ALB", new ApplicationLoadBalancerProps {
                        LoadBalancerName = "unicorn-store",
                        Vpc = ecsCluster.Vpc,
                        InternetFacing = true,
                        DeletionProtection = false,
                    }),
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = ContainerImage.FromEcrRepository(imageRepository, settings.ImageTag),
                        Environment = new Dictionary<string, string>()
                        {
                            { "ASPNETCORE_ENVIRONMENT", settings.DotNetEnvironment ?? "Production" },
                            { "DefaultAdminUsername", settings.DefaultSiteAdminUsername },
                            { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DbConnStrBuilderServerPropName}",
                                dbConstructOutput.EndpointAddress },
                            { $"UnicornDbConnectionStringBuilder__Port", dbConstructOutput.Port },
                            { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DBConnStrBuilderUserPropName}",
                                settings.DbUsername },
                        },
                        Secrets = new Dictionary<string, Secret>
                        {
                            { "DefaultAdminPassword", Helpers.CreateAutoGenPasswordSecretDef($"{settings.ScopeName}DefaultSiteAdminPassword").CreateSecret(this) },
                            { $"UnicornDbConnectionStringBuilder__{dbConstructFactory.DBConnStrBuilderPasswordPropName}", dbPasswordSecret }
                        }
                    },
                }
            );

            return ecsService;
        }
    }
}
