using Amazon.CDK.AWS.EC2;

namespace InfraAsCode.Reusable
{
    public class DatabaseConstructInfo
    {
        public string EndpointAddress { get; set; }

        public Connections_ Connections { get; set; }
    }
}
