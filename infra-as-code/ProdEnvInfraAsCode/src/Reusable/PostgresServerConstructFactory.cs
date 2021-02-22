using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;

namespace ProdEnvInfraAsCode.Reusable
{
    public class PostgresConstructFactory : DatabaseConstructFactory
    {
        public PostgresConstructFactory(UnicornStoreDeploymentEnvStackProps settings)
            :base(settings)
        {
        }

        protected override IInstanceEngine DbInstanceEngine => DatabaseInstanceEngine.POSTGRES;

        protected override IClusterEngine DbClusterEngine 
            => base.DbClusterEngine ?? DatabaseClusterEngine.AURORA_POSTGRESQL;

        protected override string ExistingAuroraDbParameterGroupName => "default.aurora-postgresql11";

        protected override InstanceClass DefaultDatabaseInstanceClass => 
            this.IsClustered ? InstanceClass.MEMORY5 : base.DefaultDatabaseInstanceClass;

        protected override InstanceSize MinimumDatabaseInstanceSize => 
            this.IsClustered ? InstanceSize.LARGE : base.MinimumDatabaseInstanceSize;
    }
}
