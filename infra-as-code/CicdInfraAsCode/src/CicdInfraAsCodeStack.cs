using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;

namespace CicdInfraAsCode
{
    public class CicdInfraAsCodeStack : Stack
    {
        public CicdInfraAsCodeStack(Construct parent, string id, IStackProps props) : base(parent, id, props)
        {
            const string appRestartLambdaFolder = "assets/lambda/ecs-container-recycle";
            //Code appRestartLambda = Code.FromAsset(appRestartLambdaFolder); // Use when running from Visual Studio
            Code appRestartLambda = Code.FromAsset($"src/{appRestartLambdaFolder}"); // Use when running from command line

            new Function(this, "HelloFunc",
                new FunctionProps
                {
                    Runtime = Runtime.NODEJS_8_10,
                    Code = appRestartLambda,
                    Handler = "index.handler"
                });
        }
    }
}
