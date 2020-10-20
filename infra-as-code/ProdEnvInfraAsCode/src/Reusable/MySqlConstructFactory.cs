using Amazon.CDK.AWS.RDS;

namespace ProdEnvInfraAsCode.Reusable
{
    public class MySqlConstructFactory : DatabaseConstructFactory
    {
        public MySqlConstructFactory(UnicornStoreDeploymentEnvStackProps settings)
            : base(settings)
        {
        }

        protected override IInstanceEngine DbInstanceEgnine => DatabaseInstanceEngine.MYSQL;

        protected override IClusterEngine DbClusterEgnine => 
            base.DbClusterEgnine ?? DatabaseClusterEngine.AURORA_MYSQL;

        protected override string ExistingAuroraDbParameterGroupName => "default.aurora-mysql5.7";

        internal override string DBConnStrBuilderUserPropName => "UserID";
    }
}