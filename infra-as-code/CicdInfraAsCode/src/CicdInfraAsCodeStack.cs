using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using CdkLib;

namespace CicdInfraAsCode
{
    public class CicdInfraAsCodeStack : Stack
    {
        public CicdInfraAsCodeStack(Construct parent, string id, IStackProps props) 
            : base(parent, id, props)
        {
            new Function(this, "EcsAppRestartWithNewImage",
                new FunctionProps
                {
                    Runtime = Runtime.NODEJS_8_10,
                    Code = Code.FromAsset("assets/lambda/ecs-container-recycle"),
                    Handler = "index.handler",
                    
                    InitialPolicy = CdkExtensions.FromPolicyProps(
                        new PolicyStatementProps
                        {   // Allow talking to CodePipeline
                            Actions = new [] { "codepipeline:PutJobSuccessResult", "codepipeline:PutJobFailureResult" },
                            Resources = new [] { "*" }
                        },
                        new PolicyStatementProps
                        {   // Allow stopping ECS Tasks
                            Actions = new[] { "ecs:ListTasks", "ecs:StopTask", "ecs:DescribeTasks" },
                            Resources = new[] { "*" }
                        }
                    )
                });
        }
    }
}
