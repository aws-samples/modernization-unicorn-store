using Amazon.CDK;
using System.Collections.Generic;

namespace InfraAsCode
{
    /// <summary>
    /// Combines implementation of the IStackProps containing 
    /// required and optional stack configuration data,
    /// with custom stack configuration settings.
    /// </summary>
    public class UnicornStoreFargateStackProps : StackProps
    {
        public enum InfrastructureType
        {
            EscFargate,
            //EKS
        }

        public static string DefaultScopeName = "UnicornStore";

        /// <summary>
        /// Target deployment infrastructure type
        /// </summary>
        public InfrastructureType Infrastructure { get; set; } = InfrastructureType.EscFargate;

        /// <summary>
        /// This is the string containing prefix for many different 
        /// CDK Construct names/IDs. Default value is "UnicornStore".
        /// </summary>
        public string ScopeName { get; set; } = DefaultScopeName;

        public string DockerImageRepository { get; set; } = "modernization-unicorn-store";

        /// <summary>
        /// ECR/Docker image label
        /// </summary>
        public string ImageTag { get; set; } = "mysql";

        public string ImageWithTag => $"{this.DockerImageRepository}:{this.ImageTag}";

        public int MaxAzs { get; set; } = 3;

        /// <summary>
        /// Please note that CPU and Memory values are interdependent and not arbitrary.
        /// See https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
        /// </summary>
        public int CpuMillicores { get; set; } = 256;

        public int DesiredReplicaCount { get; set; } = 1;

        /// <summary>
        /// Please note that CPU and Memory values are interdependent and not arbitrary.
        /// See https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
        /// </summary>
        public int MemoryMiB { get; set; } = 512;

        public bool PublicLoadBalancer { get; internal set; } = false;

        public string DotNetEnvironment { get; set; } = "Production";

        public UnicornStoreFargateStackProps()
        {
            if (this.Tags == null)
                this.Tags = new Dictionary<string, string>();
        }
    }
}
