using Amazon.CDK.AWS.EC2;
using CdkLib;
using ProdEnvInfraAsCode.Reusable;
using System;

namespace ProdEnvInfraAsCode
{
    /// <summary>
    /// Combines implementation of the IStackProps containing 
    /// required and optional stack configuration data,
    /// with custom stack configuration settings.
    /// </summary>
    public class UnicornStoreDeploymentEnvStackProps : BetterStackProps
    {
        public UnicornStoreDeploymentEnvStackProps()
            : base("UnicornStore")
        {
        }

        public enum InfrastructureType
        {
            EscFargate,
            //EKS
        }

        /// <summary>
        /// IMPORTANT: these must match Project Configuration names, 
        /// as they are used for Docker images labels.
        /// </summary>
        public enum DbEngineType
        {
            Postgres,
            SqlServer
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
            AuroraServerless // Not implemented due to the CDK not supporting it (https://github.com/aws/aws-cdk/issues/929)
        }

        public DbEngineType DbEngine { get; set; } =
#if POSTGRES
            DbEngineType.Postgres;
#else
            DbEngineType.SqlServer;
#endif

        /// <summary>
        /// Target deployment infrastructure type
        /// </summary>
        public InfrastructureType Infrastructure { get; set; } = InfrastructureType.EscFargate;

        public string DockerImageRepository { get; set; } = "unicorn-store-app";

        /// <summary>
        /// ECR/Docker image label
        /// </summary>
        public string ImageTag => this.DbEngine.ToString();

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

        public string DotNetEnvironment { get; set; }

        public string DbUsername { get; set; } = "dbadmin";

        public string DefaultSiteAdminUsername { get; set; }

        public InstanceClass? DatabaseInstanceClass { get; set; }

        public InstanceSize? DatabaseInstanceSize { get; set; }

        public SqlServerEditionType SqlServerEdition { get; set; } = SqlServerEditionType.Web;

        public RdsType RdsKind { get; set; } = RdsType.AuroraServerless;

        public SubnetType DbSubnetType { get; set; } = SubnetType.PRIVATE;

        private string ecsClusterName;

        public string EcsClusterName
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.ecsClusterName) ?
                    $"{this.ScopeName}{this.Infrastructure}Cluster"
                    : this.ecsClusterName;
            }
            set { this.ecsClusterName = value; }
        }

        internal DatabaseConstructFactory CreateDbConstructFactory()
        {
            switch(this.DbEngine)
            {
                case DbEngineType.Postgres:
                    return new PostgresConstructFactory(this);
                case DbEngineType.SqlServer:
                    return new SqlServerConstructFactory(this);
            }

            throw new NotImplementedException($"Database engine \"{this.DbEngine}\" is not supported");
        }

        protected override void PostLoadUpdate()
        {
            if (string.IsNullOrWhiteSpace(this.DotNetEnvironment))
                this.DotNetEnvironment = this.IsDebug ? "Development" : "Production";
        }
    }
}
