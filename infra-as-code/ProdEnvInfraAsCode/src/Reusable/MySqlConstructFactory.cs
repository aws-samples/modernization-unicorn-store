using Amazon.CDK.AWS.RDS;

namespace ProdEnvInfraAsCode.Reusable
{
    public class MySqlConstructFactory : DatabaseConstructFactory
    {
        public MySqlConstructFactory(UnicornStoreProdEnvStackProps settings)
            :base(settings)
        {
        }

        protected override DatabaseInstanceEngine DbInstanceEgnine => DatabaseInstanceEngine.MYSQL;

        protected override DatabaseClusterEngine DbClusterEgnine => 
            base.DbClusterEgnine ?? DatabaseClusterEngine.AURORA_MYSQL;

        protected override string ExistingAuroraDbParameterGroupName => "default.aurora-mysql5.7";

        internal override string DBConnStrBuilderUserPropName => "UserID";
    }
}
