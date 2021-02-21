using Amazon.CDK.AWS.RDS;

namespace ProdEnvInfraAsCode.Reusable
{
    public class MySqlConstructFactory : DatabaseConstructFactory
    {
        public MySqlConstructFactory(UnicornStoreDeploymentEnvStackProps settings)
            : base(settings)
        {
        }

        protected override IInstanceEngine DbInstanceEngine => DatabaseInstanceEngine.MYSQL;

        protected override IClusterEngine DbClusterEngine => 
            base.DbClusterEngine ?? DatabaseClusterEngine.AURORA_MYSQL;

        protected override string ExistingAuroraDbParameterGroupName => "default.aurora-mysql5.7";

        internal override string DBConnStrBuilderUserPropName => "UserID";
    }
}