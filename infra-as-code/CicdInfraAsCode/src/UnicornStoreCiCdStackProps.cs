using Amazon.CDK.AWS.CodeBuild;
using CdkLib;

namespace CicdInfraAsCode
{
    public class UnicornStoreCiCdStackProps : BetterStackProps
    {
        public UnicornStoreCiCdStackProps() : base("UnicornCiCdPipeline") {}

        public string DockerImageRepository { get; set; } = "unicorn-store-app";
        
        public int UntaggedImageExpirationDays { get; set; } = 3;

        public string EcsClusterName { get; set; } = "UnicornSuperstoreEscFargateCluster";

        public ComputeType BuildInstanceSize { get; set; } = ComputeType.SMALL;

        public string GitBranchToBuild { get; set; } = "master";
    }
}
