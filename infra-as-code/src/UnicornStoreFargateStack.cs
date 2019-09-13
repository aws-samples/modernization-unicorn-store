using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using System.Collections.Generic;
using SecMan = Amazon.CDK.AWS.SecretsManager;

namespace InfraAsCode
{
    public class UnicornStoreFargateStack : Stack
    {
        public UnicornStoreFargateStack(Construct parent, string id, UnicornStoreFargateStackProps stackProps) : base(parent, id, stackProps)
        {
            // TODO: Add database creation

            var vpc = new Vpc(this, $"{stackProps.ScopeName}VPC", new VpcProps { MaxAzs = stackProps.MaxAzs });

            var cluster = new Cluster(this, $"{stackProps.ScopeName}{stackProps.Infrastructure}Cluster",
                new ClusterProps {
                    Vpc = vpc,
                    ClusterName = $"{stackProps.ScopeName}{stackProps.Infrastructure}Cluster",
                }
            );

            // TODO: replace existing ECR with one created by the Stack
            var imageRepository = Repository.FromRepositoryName(this, "ExistingEcrRepository", stackProps.DockerImageRepository);

            new ApplicationLoadBalancedFargateService(this, $"{stackProps.ScopeName}FargateService",
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
                        { "ASPNETCORE_ENVIRONMENT", stackProps.DotNetEnvironment ?? "Production" }
                    },
                    Secrets = new Dictionary<string, Secret>()
                    {
                        // TODO: replace references to these pre-existing secrets with secrets created by the Stack.
                        { "DefaultAdminUsername", this.SecretFromName("UnicornAppDefaultAdminUsername", "YUbKXL") },
                        { "DefaultAdminPassword", this.SecretFromName("UnicornAppDefaultAdminPassword", "eo2RzV") },
                        { "UnicornDbConnectionStringBuilder__DataSource", this.SecretFromName("UnicornAppSqlDbServer", "WWUrRw") },
                        { "UnicornDbConnectionStringBuilder__UserId", this.SecretFromName("UnicornAppSqlDbUsername", "6fAdXo") },
                        { "UnicornDbConnectionStringBuilder__Password", this.SecretFromName("UnicornAppSqlDbPassword", "Gw8ND5") }
                    }
                }
            );
        }

        private Secret SecretFromName(string secretName, string existingSecretRandom6)
        {
            var arnComponents = new ArnComponents
            {
                Service = "secretsmanager",
                Resource = "secret",
                ResourceName = $"{secretName}-{existingSecretRandom6}",
                Sep = ":"
            };
            string secretArn = Arn.Format(arnComponents, this);

            var smSecret = SecMan.Secret.FromSecretArn(this, secretName, secretArn);
            var secret = Secret.FromSecretsManager(smSecret);
            return secret;
        }
    }
}
