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
            Repository dockerRepo = this.CreateDockerRepo(settings);
            Vcs.Repository gitRepo = this.CreateVersionControlRepo(settings);
            this.CreateCiCdPipeline(settings, dockerRepo, gitRepo);
        }

        private Vcs.Repository CreateVersionControlRepo(UnicornStoreCiCdStackProps settings)
        {
            return new Vcs.Repository(this, "CodeCommitRepo",
                new Vcs.RepositoryProps
                {
                    RepositoryName = $"{settings.ScopeName}GitRepo",
                    Description = $"Version control system for {settings.ScopeName} application"
                }
            );
        }

        private Pipeline CreateCiCdPipeline(UnicornStoreCiCdStackProps settings, Repository dockerRepo, Vcs.Repository gitRepo)
        {
            var sourceOutput = new Artifact_($"Unicorn-Store-VS-Solution");

            var buildPipeline = new Pipeline(this, "BuildPipeline",
                new PipelineProps
                {
                    PipelineName = settings.ScopeName,
                    Stages = new []
                    {
                        CdkExtensions.StageFromActions("Source",
                            new CodeCommitSourceAction(new CodeCommitSourceActionProps{
                                ActionName = "Git-checkout-from-CodeCommit-repo",
                                Repository = gitRepo,
                                Output = sourceOutput,
                                Branch = settings.GitBranchToBuild,
                            })
                        ),
                        CdkExtensions.StageFromActions("Build",
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                Input = sourceOutput,
                                ActionName = "Build-Docker-image",
                                Type = CodeBuildActionType.BUILD,
                                Project = new PipelineProject(this, "CodeBuildProject", new PipelineProjectProps
                                {
                                    ProjectName = $"{settings.ScopeName}-App-Docker-image-build",
                                    BuildSpec = BuildSpec.FromSourceFilename("./infra-as-code/CicdInfraAsCode/src/assets/codebuild/buildpec.yaml"), // <= path relative to the git repo root
                                    Environment = new BuildEnvironment{
                                        Privileged = true,
                                        BuildImage = LinuxBuildImage.UBUNTU_14_04_DOCKER_18_09_0,
                                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                                        {
                                            {   // Tells the Buildspec where to push images produced during the build
                                                "DockerRepoUri", new BuildEnvironmentVariable { Value = dockerRepo.RepositoryArn }
                                            },
                                            { "DbEngine", new BuildEnvironmentVariable { Value = settings.DbEngine.ToString() } },
                                            { "BuildConfig", new BuildEnvironmentVariable { Value = settings.BuildConfiguration } }
                                        },
                                        ComputeType = settings.BuildInstanceSize
                                    }
                                })
                            })
                        )
                    }
                }
            );
            
            Function appRestartLambda = this.CreateLambdaForRestartingEcsApp();

            return buildPipeline;
        }

        private Repository CreateDockerRepo(UnicornStoreCiCdStackProps settings)
        {
            return new Repository(this, "DockerImageRepository",
                new RepositoryProps
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

        private Function CreateLambdaForRestartingEcsApp()
        {
            return new Function(this, "EcsAppRestartWithNewImage",
                new FunctionProps
                {
                    Runtime = Runtime.NODEJS_8_10,
                    Code = Code.FromAsset("assets/lambda/ecs-container-recycle"),
                    Handler = "index.handler",

                    InitialPolicy = CdkExtensions.FromPolicyProps(
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
