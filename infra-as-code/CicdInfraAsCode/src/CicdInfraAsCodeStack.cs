using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.CodePipeline;
using CdkLib;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Vcs = Amazon.CDK.AWS.CodeCommit;
using Amazon.CDK.AWS.CodeBuild;
using System.Collections.Generic;

namespace CicdInfraAsCode
{
    public class CicdInfraAsCodeStack : Stack
    {
        protected readonly UnicornStoreCiCdStackProps settings;

        public CicdInfraAsCodeStack(Construct parent, string id, UnicornStoreCiCdStackProps settings)
            : base(parent, id, settings)
        {
            this.settings = settings;

            Repository dockerImageRepo = this.CreateDockerImageRepo();
            Vcs.Repository gitRepo = this.CreateVersionControlRepo();
            this.CreateCiCdPipeline(dockerImageRepo, gitRepo);
        }

        private Vcs.Repository CreateVersionControlRepo()
        {
            return new Vcs.Repository(this, "CodeCommitRepo",
                new Vcs.RepositoryProps
                {
                    RepositoryName = this.settings.CodeCommitRepoName,
                    Description = $"Version control system for {this.settings.ScopeName} application"
                }
            );
        }

        private Pipeline CreateCiCdPipeline(Repository dockerRepo, Vcs.Repository gitRepo)
        {
            Artifact_ sourceCodeArtifact = new Artifact_("Unicorn-Store-Visual-Studio-Solution");

            var buildPipeline = new Pipeline(this, "BuildPipeline",
                new PipelineProps
                {
                    PipelineName = this.settings.ScopeName,
                    Stages = new Amazon.CDK.AWS.CodePipeline.IStageProps[]
                    {
                        Helpers.StageFromActions("Source", CreateSourceVcsCheckoutAction(gitRepo, sourceCodeArtifact)),
                        Helpers.StageFromActions("Build",
                            this.CreateDockerImageBuildAction(dockerRepo, sourceCodeArtifact),
                            this.CreateAppDeploymentEnvironmentBuildAction(sourceCodeArtifact)
                        ),
                        Helpers.StageFromActions("Restart-the-App", this.CreateLambdaInvokeAction())
                    }
                }
            );

            return buildPipeline;
        }

        private CodeCommitSourceAction CreateSourceVcsCheckoutAction(Vcs.Repository gitRepo, Artifact_ sourceOutput)
        {
            return new CodeCommitSourceAction(new CodeCommitSourceActionProps
            {
                ActionName = "Git-checkout-from-CodeCommit-repo",
                Repository = gitRepo,
                Output = sourceOutput,
                Branch = this.settings.GitBranchToBuild,
            });
        }

        private CodeBuildAction CreateDockerImageBuildAction(Repository dockerRepo, Artifact_ sourceOutput)
        {
            var codeBuildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                Input = sourceOutput,
                ActionName = "Build-app-Docker-image",
                Type = CodeBuildActionType.BUILD,
                Project = new PipelineProject(this, "CodeBuildProject", new PipelineProjectProps
                {
                    ProjectName = "Unicorn-Store-app-Docker-image-build",
                    BuildSpec = new BuildSpecHelper 
                    { 
                        PreBuildCommands = new []
                        {
                            "echo Logging in to Amazon ECR...",
                            "aws --version",
                            "AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)",
                            "aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com"
                        },
                        BuildCommands = new []
                        {
                            "docker build --build-arg BUILD_CONFIG=${BuildConfig}${DbEngine} -t ${DockerRepoUri}:${ImageTag} ."
                        },
                        PostBuildCommands = new []
                        {
                            "docker push ${DockerRepoUri}:${ImageTag}"
                        }
                    }.ToBuildSpec(),

                    Environment = new BuildEnvironment
                    {
                        Privileged = true,
                        BuildImage = LinuxBuildImage.STANDARD_5_0,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                        {
                            {   // Tells the Buildspec where to push images produced during the build
                                "DockerRepoUri", new BuildEnvironmentVariable { Value = dockerRepo.RepositoryUriForTag() }
                            },
                            { "DbEngine", new BuildEnvironmentVariable { Value = this.settings.DbEngine.ToString() } },
                            { "BuildConfig", new BuildEnvironmentVariable { Value = this.settings.BuildConfiguration } },
                            { "ImageTag", new BuildEnvironmentVariable { Value = this.settings.ImageTag } }
                        },
                        ComputeType = this.settings.BuildInstanceSize
                    },
                    Role = new Role(this, "Docker-Image-build-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        ManagedPolicies = Helpers.FromAwsManagedPolicies("AmazonEC2ContainerRegistryPowerUser")
                    }),
                    Cache = Cache.Local(LocalCacheMode.SOURCE, LocalCacheMode.DOCKER_LAYER)
                })
            });

            return codeBuildAction;
        }

        private CodeBuildAction CreateAppDeploymentEnvironmentBuildAction(Artifact_ sourceOutput)
        {
            var codeBuildAction = new CodeBuildAction(new CodeBuildActionProps
            {
                Input = sourceOutput,
                ActionName = "Build-app-deployment-environment",
                Type = CodeBuildActionType.BUILD,
                Project = new PipelineProject(this, "DeploymentEnvCreationProject", new PipelineProjectProps
                {
                    ProjectName = "Unicorn-Store-deployment-env-build",
                    BuildSpec = new BuildSpecHelper
                    {
                        InstallRuntimes = new Dictionary<string, string>()
                        {
                            { "nodejs", "14" }, // Make sure this matches Lambda runtime version specified in CreateLambdaForRestartingEcsApp()
                            { "dotnet", "5.0" }
                        },
                        PreBuildCommands = new [] 
                        {
                            "aws --version",
                            "npm install -g aws-cdk${CdkVersion}",
                            "cdk --version",

                            //// Install .NET Core 3 SDK. TODO: remove this section after dotnet 3.0 runtime becomes available on AWS Linux CodeBuild images
                            //"wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                            //"dpkg -i packages-microsoft-prod.deb",

                            //"apt-get update",
                            //"apt-get -y install apt-transport-https",
                            //"apt-get update",
                            //"apt-get -y install dotnet-sdk-3.0",

                            "dotnet --list-sdks",
                            "dotnet --info",
                            "dotnet --version"
                        },
                        BuildCommands = new []
                        {
                            "echo Building CDK CI/CD project",
                            "cd ./infra-as-code/ProdEnvInfraAsCode/src",
                            "dotnet build ProdEnvInfraAsCode.csproj -c ${BuildConfig}${DbEngine}",
                            "cdk diff || true",
                            "cdk deploy --require-approval never"
                        }
                    }.ToBuildSpec(),

                    Environment = new BuildEnvironment
                    {
                        Privileged = true,
                        BuildImage = LinuxBuildImage.STANDARD_5_0,
                        ComputeType = this.settings.BuildInstanceSize,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                        {
                            { "DbEngine", new BuildEnvironmentVariable { Value = this.settings.DbEngine.ToString() } },
                            { "BuildConfig", new BuildEnvironmentVariable { Value = this.settings.BuildConfiguration } },
                            { "DockerImageRepository", new BuildEnvironmentVariable { Value = this.settings.DockerImageRepository } },
                            { "DotNetEnvironment", new BuildEnvironmentVariable { Value = this.settings.IsDebug ? "Development" : "Production"} },
                            { "EcsClusterName", new BuildEnvironmentVariable { Value = this.settings.AppEcsClusterName } },
                            { "CdkVersion", new BuildEnvironmentVariable { Value = this.settings.CdkInstallCommandVersion } }
                        }
                    },
                    Role = new Role(this, "App-deployment-env-creation-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        ManagedPolicies = Helpers.FromAwsManagedPolicies(
                            //"CloudWatchLogsFullAccess", 
                            //"AWSCodeDeployRoleForECSLimited",
                            //"AWSCloudFormationFullAccess"
                            "AdministratorAccess" // <= TODO: Find more fine-grained set of policies to enable CloudFormation stack creation
                        ) 
                    }),
                    Cache = Cache.Local(LocalCacheMode.SOURCE, LocalCacheMode.DOCKER_LAYER)
                })
            });

            return codeBuildAction;
        }

        private Repository CreateDockerImageRepo()
        {
            return new Repository(this, "DockerImageRepository", new RepositoryProps
                {
                    RepositoryName = this.settings.DockerImageRepository,
                    // RemovalPolicy = RemovalPolicy.DESTROY, // Destroy can only destroy empty repos. Ones with images will cause stack deletion to fail, requiring more manual cleanup.
                    LifecycleRules = new ILifecycleRule[]
                    {
                        new LifecycleRule
                        {
                            Description = $"Expire untagged images in {this.settings.UntaggedImageExpirationDays} days",
                            TagStatus = TagStatus.UNTAGGED,
                            MaxImageAge = Duration.Days(this.settings.UntaggedImageExpirationDays),
                        }
                    }
                }
            );
        }

        private LambdaInvokeAction CreateLambdaInvokeAction()
        {
            string ecsClusterArn = this.FormatArn(new ArnComponents
            {
                Service = "ecs",
                Resource = "cluster",
                ResourceName = this.settings.AppEcsClusterName
            });

            var lambdaInvokeAction = new LambdaInvokeAction(new LambdaInvokeActionProps
            {
                ActionName = "Recycle-ECS-Cluster-Tasks",
                Lambda = this.CreateLambdaForRestartingEcsApp(),
                UserParameters = new Dictionary<string, object>() { { "clusterArn", ecsClusterArn } }
            });

            return lambdaInvokeAction;
        }

        private Function CreateLambdaForRestartingEcsApp()
        {
            return new Function(this, "EcsAppRestartWithNewImage",
                new FunctionProps
                {
                    FunctionName = $"Refresh-ECS-Cluster-From-CodePipeline",
                    Runtime = Runtime.NODEJS_10_X, // Ensure this matches the "runtime" installed as a part of the buildspec (see CreateAppDeploymentEnvironmentBuildAction() method above)
                    Code = Code.FromAsset("assets/lambda/ecs-container-recycle"),
                    Handler = "index.handler", // Points to the NodeJs sub-project's "index.js" file, and the "handler()" function in it: "exports.handler = async (event, context) => {..."

                    InitialPolicy = Helpers.FromPolicyProps(
                        new PolicyStatementProps
                        {   // Allow talking to CodePipeline
                            Actions = new[] { "codepipeline:PutJobSuccessResult", "codepipeline:PutJobFailureResult" },
                            Resources = new[] { "*" }
                        },
                        new PolicyStatementProps
                        {   // Allow stopping ECS Tasks
                            Actions = new[] { "ecs:ListServices", "ecs:UpdateService" },
                            Resources = new[] { "*" }
                        }
                    )
                }
            );
        }
    }
}
