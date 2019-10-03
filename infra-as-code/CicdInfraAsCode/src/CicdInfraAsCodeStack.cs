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
        public CicdInfraAsCodeStack(Construct parent, string id, UnicornStoreCiCdStackProps settings)
            : base(parent, id, settings)
        {
            Repository dockerImageRepo = this.CreateDockerImageRepo(settings);
            Vcs.Repository gitRepo = this.CreateVersionControlRepo(settings);
            this.CreateCiCdPipeline(settings, dockerImageRepo, gitRepo);
        }

        private Vcs.Repository CreateVersionControlRepo(UnicornStoreCiCdStackProps settings)
        {
            return new Vcs.Repository(this, "CodeCommitRepo",
                new Vcs.RepositoryProps
                {
                    RepositoryName = "Unicorn-Store-Sample-Git-Repo",
                    Description = $"Version control system for {settings.ScopeName} application"
                }
            );
        }

        private Pipeline CreateCiCdPipeline(UnicornStoreCiCdStackProps settings, Repository dockerRepo, Vcs.Repository gitRepo)
        {
            var sourceCodeArtifact = new Artifact_("Unicorn-Store-Visual-Studio-Solution");

            var buildPipeline = new Pipeline(this, "BuildPipeline",
                new PipelineProps
                {
                    PipelineName = settings.ScopeName,
                    Stages = new[]
                    {
                        Helpers.StageFromActions("Source", CreateSourceVcsCheckoutAction(settings, gitRepo, sourceCodeArtifact)),
                        Helpers.StageFromActions("Build", 
                            this.CreateDockerImageBuildAction(settings, dockerRepo, sourceCodeArtifact),
                            this.CreateAppDeploymentEnvironmentBuildAction(settings, dockerRepo, sourceCodeArtifact)
                        ),
                        Helpers.StageFromActions("Restart-the-App", CreateLambdaInvokeAction(settings))
                    }
                }
            );

            return buildPipeline;
        }

        private static CodeCommitSourceAction CreateSourceVcsCheckoutAction(UnicornStoreCiCdStackProps settings, Vcs.Repository gitRepo, Artifact_ sourceOutput)
        {
            return new CodeCommitSourceAction(new CodeCommitSourceActionProps
            {
                ActionName = "Git-checkout-from-CodeCommit-repo",
                Repository = gitRepo,
                Output = sourceOutput,
                Branch = settings.GitBranchToBuild,
            });
        }

        private CodeBuildAction CreateDockerImageBuildAction(UnicornStoreCiCdStackProps settings, Repository dockerRepo, Artifact_ sourceOutput)
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
                            "$(aws ecr get-login --region $AWS_DEFAULT_REGION --no-include-email)"
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
                        BuildImage = LinuxBuildImage.UBUNTU_14_04_DOCKER_18_09_0,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                        {
                            {   // Tells the Buildspec where to push images produced during the build
                                "DockerRepoUri", new BuildEnvironmentVariable { Value = dockerRepo.RepositoryUriForTag() }
                            },
                            { "DbEngine", new BuildEnvironmentVariable { Value = settings.DbEngine.ToString() } },
                            { "BuildConfig", new BuildEnvironmentVariable { Value = settings.BuildConfiguration } },
                            { "ImageTag", new BuildEnvironmentVariable { Value = settings.ImageTag } }
                        },
                        ComputeType = settings.BuildInstanceSize
                    },
                    Role = new Role(this, "Docker-Image-build-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        RoleName = $"{settings.ScopeName}-Build-Docker-Image-Role",
                        ManagedPolicies = Helpers.FromAwsManagedPolicies("AmazonEC2ContainerRegistryPowerUser")
                    }),
                    Cache = Cache.Local(LocalCacheMode.SOURCE, LocalCacheMode.DOCKER_LAYER)
                })
            });

            return codeBuildAction;
        }

        private CodeBuildAction CreateAppDeploymentEnvironmentBuildAction(UnicornStoreCiCdStackProps settings, Repository dockerRepo, Artifact_ sourceOutput)
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
                            { "nodejs", "8" },
                            { "dotnet", "2.2" }
                        },
                        PreBuildCommands = new [] 
                        {
                            "aws --version",
                            "npm install -g aws-cdk",
                            "cdk --version"
                        },
                        BuildCommands = new []
                        {
                            "echo Building CDK CI/CD project",
                            "cd ./infra-as-code/ProdEnvInfraAsCode",
                            "dotnet build ./src/ProdEnvInfraAsCode.csproj -c ${BuildConfig}${DbEngine}",
                            "cdk diff || true",
                            "cdk deploy --require-approval never"
                        }
                    }.ToBuildSpec(),

                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.STANDARD_2_0,
                        ComputeType = settings.BuildInstanceSize,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                        {
                            { "DbEngine", new BuildEnvironmentVariable { Value = settings.DbEngine.ToString() } },
                            { "BuildConfig", new BuildEnvironmentVariable { Value = settings.BuildConfiguration } },
                            { "DockerImageRepository", new BuildEnvironmentVariable { Value = settings.DockerImageRepository } },
                            { "DotNetEnvironment", new BuildEnvironmentVariable { Value = settings.IsDebug ? "Development" : "Production"} },
                            { "EcsClusterName", new BuildEnvironmentVariable { Value = settings.AppEcsClusterName } }
                        }
                    },
                    Role = new Role(this, "App-deployment-env-creation-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        RoleName = $"{settings.ScopeName}-Build-Deployment-Env-Role",
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

        private Repository CreateDockerImageRepo(UnicornStoreCiCdStackProps settings)
        {
            return new Repository(this, "DockerImageRepository", new RepositoryProps
                {
                    RepositoryName = settings.DockerImageRepository,
                    LifecycleRules = new []
                    {
                        new LifecycleRule
                        {
                            Description = $"Expire untagged images in {settings.UntaggedImageExpirationDays} days",
                            TagStatus = TagStatus.UNTAGGED,
                            MaxImageAge = Duration.Days(settings.UntaggedImageExpirationDays),
                        }
                    }
                }
            );
        }

        private LambdaInvokeAction CreateLambdaInvokeAction(UnicornStoreCiCdStackProps settings)
        {
            string ecsClusterArn = this.FormatArn(new ArnComponents
            {
                Service = "ecs",
                Resource = "cluster",
                ResourceName = settings.AppEcsClusterName
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
                    FunctionName = $"Stop-ECS-Cluster-Tasks-From-CodePipeline",
                    Runtime = Runtime.NODEJS_8_10,
                    Code = Code.FromAsset("assets/lambda/ecs-container-recycle"),
                    Handler = "index.handler",

                    InitialPolicy = Helpers.FromPolicyProps(
                        new PolicyStatementProps
                        {   // Allow talking to CodePipeline
                            Actions = new[] { "codepipeline:PutJobSuccessResult", "codepipeline:PutJobFailureResult" },
                            Resources = new[] { "*" }
                        },
                        new PolicyStatementProps
                        {   // Allow stopping ECS Tasks
                            Actions = new[] { "ecs:ListTasks", "ecs:StopTask", "ecs:DescribeTasks" },
                            Resources = new[] { "*" }
                        }
                    )
                }
            );
        }
    }
}
