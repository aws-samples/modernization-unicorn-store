using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.RDS;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;

namespace InfraAsCode
{
    public class UnicornStoreFargateStack : Stack
    {
        public UnicornStoreFargateStack(Construct parent, string id, UnicornStoreFargateStackProps stackProps) : base(parent, id, stackProps)
        {
            var siteAdminPasswordSecret = this.AutoGenPasswordSecret($"{stackProps.ScopeName}DefaultSiteAdminPassword");

            var vpc = new Vpc(this, $"{stackProps.ScopeName}VPC", new VpcProps { MaxAzs = stackProps.MaxAzs });

            var database = new DatabaseInstance(this, $"{stackProps.ScopeName}Database",
                new DatabaseInstanceProps
                {
                    Engine = DatabaseInstanceEngine.SQL_SERVER_EX,
                    InstanceClass = InstanceType.Of(InstanceClass.BURSTABLE2, InstanceSize.SMALL),
                    MasterUsername = stackProps.DbUsername,
                    Vpc = vpc,
                    InstanceIdentifier = $"{stackProps.ScopeName}Database",
                    DeletionProtection = stackProps.DotNetEnvironment != "Development",
                    //DatabaseName = $"{stackProps.ScopeName}", // Can't be specified, at least not for SQL Server
                }
            );

            var cluster = new Cluster(this, $"{stackProps.ScopeName}{stackProps.Infrastructure}Cluster",
                new ClusterProps {
                    Vpc = vpc,
                    ClusterName = $"{stackProps.ScopeName}{stackProps.Infrastructure}Cluster",
                }
            );

            // TODO: replace existing ECR with one created by the Stack
            var imageRepository = Repository.FromRepositoryName(this, "ExistingEcrRepository", stackProps.DockerImageRepository);

            var secService = new ApplicationLoadBalancedFargateService(this, $"{stackProps.ScopeName}FargateService",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster,
                    DesiredCount = stackProps.DesiredReplicaCount,
                    Cpu = stackProps.CpuMillicores,
                    MemoryLimitMiB = stackProps.MemoryMiB,
                    Image = ContainerImage.FromEcrRepository(imageRepository, stackProps.ImageTag),
                    PublicLoadBalancer = stackProps.PublicLoadBalancer,
                    Environment = new Dictionary<string, string>()
                    {
                        { "ASPNETCORE_ENVIRONMENT", stackProps.DotNetEnvironment ?? "Production" },
                        { "DefaultAdminUsername", stackProps.DefaultSiteAdminUsername },
                        { "UnicornDbConnectionStringBuilder__DataSource", database.DbInstanceEndpointAddress }, // <- TODO: SQL Server specific, needs to be made parameter-driven
                        { "UnicornDbConnectionStringBuilder__UserId", stackProps.DbUsername }, // <- TODO: SQL Server specific, needs to be made parameter-driven
                        { "DefaultAdminPassword", "bad-idea!" } // <- TODO: Remove this temporary solution after the "Parameter Mismatch" issue is resolved for auto-generated password secret
                    },
                    Secrets = new Dictionary<string, Secret>()
                    {
                        //{ "DefaultAdminPassword", siteAdminPasswordSecret }, // TODO: Figure out why this produces "Parameter Mismatch" error
                        { "UnicornDbConnectionStringBuilder__Password", Secret.FromSecretsManager(database.Secret) } // <- TODO: SQL Server specific, needs to be made parameter-driven
                    }
                }
            );
            

            database.Connections.AllowDefaultPortFrom(secService.Service.Connections.SecurityGroups[0]);
        }

        private Secret AutoGenPasswordSecret(string secretName)
        {
            var smSecret = new SecMan.Secret(this, secretName,
                new SecMan.SecretProps
                {
                    SecretName = secretName,
                    GenerateSecretString = new SecMan.SecretStringGenerator
                    {
                        PasswordLength = 8,
                    }
                }
            );
            var secret = Secret.FromSecretsManager(smSecret);
            return secret;
        }
    }
}
