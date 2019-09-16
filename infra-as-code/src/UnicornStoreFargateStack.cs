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
            Secret siteAdminPasswordSecret = this.AutoGenPasswordSecret($"{stackProps.ScopeName}DefaultSiteAdminPassword");
            SecMan.Secret databaseMasterPasswordSecret = this.GeneratePasswordSecret($"{stackProps.ScopeName}DatabasePassword");

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
                    MasterUserPassword = databaseMasterPasswordSecret.SecretValue
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
                        //{ "DefaultAdminPassword", "bad-idea!" } // <- TODO: Remove this temporary solution after the "Parameter Mismatch" issue is resolved for auto-generated password secret
                    },
                    Secrets = new Dictionary<string, Secret>()
                    {
                        { "DefaultAdminPassword", this.WrapSecretBug(siteAdminPasswordSecret, "SiteAdminPassword") }, // TODO: Figure out why this produces "Parameter Mismatch" error
                        { "UnicornDbConnectionStringBuilder__Password", this.WrapSecretBug(Secret.FromSecretsManager(databaseMasterPasswordSecret), "DbPassword") } // <- TODO: SQL Server specific, needs to be made parameter-driven
                    }
                }
            );
            

            database.Connections.AllowDefaultPortFrom(secService.Service.Connections.SecurityGroups[0]);
        }

        /// <summary>
        /// The work-around for the "Resolution error: System.Reflection.TargetParameterCountException: Parameter count mismatch"
        /// bug when using Secrets as is
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        private Secret WrapSecretBug(Secret secret, string secretName)
        {
            var smSecret = SecMan.Secret.FromSecretArn(this, $"{secretName}BugWorkaround", secret.Arn);
            return Secret.FromSecretsManager(smSecret);
        }
        
        private SecMan.Secret GeneratePasswordSecret(string secretName)
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
            return smSecret;
        }

        private Secret AutoGenPasswordSecret(string secretName)
        {
            var smSecret = this.GeneratePasswordSecret(secretName);
            var secret = Secret.FromSecretsManager(smSecret);
            return secret;
        }
    }
}
