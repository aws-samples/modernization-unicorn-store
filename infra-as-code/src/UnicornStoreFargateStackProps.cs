using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
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

        public int DesiredReplicaCount { get; set; } = 1;

        /// <summary>
        /// Please note that CPU and Memory values are interdependent and not arbitrary.
        /// See https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html
        /// </summary>
        public int MemoryMiB { get; set; } = 512;

        public bool PublicLoadBalancer { get; set; } = false;

        public string DotNetEnvironment { get; set; } = "Production";

        public string DbUsername { get; set; } = "dbadmin";

        public string DefaultSiteAdminUsername { get; set; }

        public InstanceClass DatabaseInstanceClass { get; set; } = InstanceClass.BURSTABLE2;

        public InstanceSize DatabaseInstanceSize { get; set; } = InstanceSize.XLARGE;

        public DbEngineType DbEngine { get; set; } = DbEngineType.SQLSERVER;

        public SqlServerEditionType SqlServerEdition { get; set; } = SqlServerEditionType.Web;

        public RdsType RdsKind { get; set; } = RdsType.AuroraServerless;

        public SubnetType DbSubnetType { get; set; } = SubnetType.PRIVATE;

        internal DatabaseClusterEngine DbClusterEgnine
        {
            get
            {
                switch (this.DbEngine)
                {
                    case DbEngineType.POSTGRES:
                        return this.RdsKind == RdsType.RegularRds ? this.DbInstanceEgnine : DatabaseClusterEngine.AURORA_POSTGRESQL;
                    case DbEngineType.MYSQL:
                        return this.RdsKind == RdsType.RegularRds ? this.DbInstanceEgnine : DatabaseClusterEngine.AURORA_MYSQL;
                    default:
                        return this.DbInstanceEgnine;
                }
            }
        }

        internal DatabaseInstanceEngine DbInstanceEgnine
        {
            get
            {
                switch (this.DbEngine)
                {
                    case DbEngineType.SQLSERVER:
                        switch (this.SqlServerEdition)
                        {
                            case SqlServerEditionType.Enterprise:
                                return DatabaseInstanceEngine.SQL_SERVER_EE;
                            case SqlServerEditionType.Express:
                                return DatabaseInstanceEngine.SQL_SERVER_EX;
                            case SqlServerEditionType.Standard:
                                return DatabaseInstanceEngine.SQL_SERVER_SE;
                            case SqlServerEditionType.Web:
                                return DatabaseInstanceEngine.SQL_SERVER_WEB;
                            default:
                                throw new NotImplementedException($"Unsupported SQL Server edition \"{this.SqlServerEdition}\"");
                        }
                    case DbEngineType.POSTGRES:
                        return DatabaseInstanceEngine.POSTGRES;
                    case DbEngineType.MYSQL:
                        return DatabaseInstanceEngine.MYSQL;
                    default:
                        throw new NotImplementedException($"Database Engine \"{this.DbEngine}\" is not supported.");
                }
            }
        }

        internal string DbConnStrBuilderServerPropName =>
            this.DbEngine == DbEngineType.SQLSERVER ? "DataSource" : "Server";

        internal string DBConnStrBuilderUserPropName
        {
            get
            {
                switch(this.DbEngine)
                {
                    case DbEngineType.SQLSERVER:
                        return "UserId";
                    case DbEngineType.POSTGRES:
                        return "Username";
                    case DbEngineType.MYSQL:
                        return "UserID";
                }
                throw new NotImplementedException($"DbEngine \"{this.DbEngine}\" is not supported.");
            }
        }

        internal string DBConnStrBuilderPasswordPropName => "Password";

        public UnicornStoreFargateStackProps()
        {
            if (this.Tags == null)
                this.Tags = new Dictionary<string, string>();
        }
    }
}
