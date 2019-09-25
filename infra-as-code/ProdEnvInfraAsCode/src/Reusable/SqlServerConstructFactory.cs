using Amazon.CDK.AWS.RDS;
using System;
using static ProdEnvInfraAsCode.UnicornStoreFargateStackProps;

namespace ProdEnvInfraAsCode.Reusable
{
    public class SqlServerConstructFactory : DatabaseConstructFactory
    {
        public SqlServerConstructFactory(UnicornStoreFargateStackProps settings)
            :base(settings)
        {
        }

        protected override bool IsClustered => false;

        protected override DatabaseInstanceEngine DbInstanceEgnine
        {
            get
            {
                switch (this.Settings.SqlServerEdition)
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
                        throw new NotImplementedException($"Unsupported SQL Server edition \"{this.Settings.SqlServerEdition}\"");
                }
            }
        }

        internal override string DbConnStrBuilderServerPropName => "DataSource";

        internal override string DBConnStrBuilderUserPropName => "UserId";
    }
}
