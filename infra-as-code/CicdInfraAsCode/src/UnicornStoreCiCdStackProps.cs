﻿using Amazon.CDK.AWS.CodeBuild;
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
            Postgres,
            SqlServer
        }

        public UnicornStoreCiCdStackProps() : base("Unicorn-Store-CI-CD-Pipeline") {}

        public DbEngineType DbEngine { get; set; } =
#if POSTGRES
            DbEngineType.Postgres;
#else
            DbEngineType.SqlServer;
#endif

        public string BuildConfiguration => this.IsDebug ? "Debug" : "Release";

        public string DockerImageRepository { get; set; } = "unicorn-store-app";

        public string ImageTag => this.DbEngine.ToString();
        
        public int UntaggedImageExpirationDays { get; set; } = 3;

        public string AppEcsClusterName { get; set; } = "Unicorn-Store-ECS-Fargate-Cluster";

        public ComputeType BuildInstanceSize { get; set; } = ComputeType.SMALL;

        public string GitBranchToBuild { get; set; } = "master";

        public string CodeCommitRepoName { get; set; } = "Unicorn-Store-Sample-Git-Repo";

        public string CdkVersion { get; set; }

        internal string CdkInstallCommandVersion => string.IsNullOrWhiteSpace(this.CdkVersion) ? string.Empty : $"@{this.CdkVersion}"; // Install latest CDK if version is not specified.
    }
}
