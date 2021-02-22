using Amazon.CDK.AWS.EC2;

namespace ProdEnvInfraAsCode.Reusable
{
    public class DatabaseConstructOutput
    {
        public string EndpointAddress { get; set; }

        public Connections_ Connections { get; set; }

        public string Port { get; set; }
    }
}
