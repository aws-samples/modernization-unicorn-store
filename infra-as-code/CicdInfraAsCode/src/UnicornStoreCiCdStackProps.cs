using Amazon.CDK.AWS.CodeBuild;
using CdkLib;

namespace CicdInfraAsCode
{
    public class UnicornStoreCiCdStackProps : BetterStackProps
    {
        /// <summary>
        /// IMPORTANT: these must match Project Configuration names, 
        /// as they are used for Docker images labels.
        /// </summary>
        public enum DbEngineType
        {
            MySQL,
            Postgres,
            SqlServer
        }

        public UnicornStoreCiCdStackProps() : base("Unicorn-Store-CI-CD-Pipeline") {}

        public DbEngineType DbEngine { get; set; } =
#if MYSQL
            DbEngineType.MySQL;
#elif POSTGRES
            DbEngineType.Postgres;
#else
            DbEngineType.SqlServer;
#endif

        public bool IsDebugBuild { get; set; } =
#if Debug || DEBUG
            true;
#else
            false;
#endif

        public string BuildConfiguration => this.IsDebugBuild ? "Debug" : "Release";

        public string DockerImageRepository { get; set; } = "unicorn-store-app";
        
        public int UntaggedImageExpirationDays { get; set; } = 3;

        public string EcsClusterName { get; set; } = "UnicornSuperstoreEscFargateCluster";

        public ComputeType BuildInstanceSize { get; set; } = ComputeType.SMALL;

        public string GitBranchToBuild { get; set; } = "master";
    }
}
