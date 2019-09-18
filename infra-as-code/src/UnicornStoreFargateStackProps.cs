using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using InfraAsCode.Reusable;
using System;
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

        public enum DbEngineType
        {
            MYSQL,
            POSTGRES,
            SQLSERVER
        }

        public enum SqlServerEditionType
        {
            Express,
            Web,
            Standard,
            Enterprise
        }

        public enum RdsType
        {
            RegularRds,
            Aurora,
            AuroraServerless
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
        public string ImageTag => this.DbEngine.ToString().ToLowerInvariant();

        public string ImageWithTag => $"{this.DockerImageRepository}:{this.ImageTag}";

        public int MaxAzs { get; set; } = 3;

        /// <summary>
        /// Please note that CPU and Memory values are interdependent and not arbitrary.
        /// See https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
        /// </summary>
        public int CpuMillicores { get; set; } = 256;

        public int DesiredComputeReplicaCount { get; set; } = 1;

        /// <summary>
        /// Please note that CPU and Memory values are interdependent and not arbitrary.
        /// See https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
        /// </summary>
        public int MemoryMiB { get; set; } = 512;

        public bool PublicLoadBalancer { get; set; } = false;

        public string DotNetEnvironment { get; set; } = "Production";

        public string DbUsername { get; set; } = "dbadmin";

        public string DefaultSiteAdminUsername { get; set; }

        public InstanceClass? DatabaseInstanceClass { get; set; }

        public InstanceSize? DatabaseInstanceSize { get; set; }

        public DbEngineType DbEngine { get; set; } = DbEngineType.SQLSERVER;

        public SqlServerEditionType SqlServerEdition { get; set; } = SqlServerEditionType.Web;

        public RdsType RdsKind { get; set; } = RdsType.AuroraServerless;

        public SubnetType DbSubnetType { get; set; } = SubnetType.PRIVATE;

        public UnicornStoreFargateStackProps()
        {
            if (this.Tags == null)
                this.Tags = new Dictionary<string, string>();
        }

        internal DatabaseConstructFactory CreateDbConstructFactory()
        {
            switch(this.DbEngine)
            {
                case DbEngineType.MYSQL:
                    return new MySqlConstructFactory(this);
                case DbEngineType.POSTGRES:
                    return new PostgresConstructFactory(this);
                case DbEngineType.SQLSERVER:
                    return new SqlServerConstructFactory(this);
            }

            throw new NotImplementedException($"Database engine \"{this.DbEngine}\" is not supported");
        }
    }
}
