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
                        CdkExtensions.StageFromActions("Source", CreateSourceVcsCheckoutStage(settings, gitRepo, sourceCodeArtifact)),
                        CdkExtensions.StageFromActions("Build", 
                            this.CreateDockerImageBuildAction(settings, dockerRepo, sourceCodeArtifact),
                            this.CreateAppDeploymentEnvironmentBuildAction(settings, dockerRepo, sourceCodeArtifact)
                        )
                    }
                }
            );

            Function appRestartLambda = this.CreateLambdaForRestartingEcsApp();

            return buildPipeline;
        }

        private static CodeCommitSourceAction CreateSourceVcsCheckoutStage(UnicornStoreCiCdStackProps settings, Vcs.Repository gitRepo, Artifact_ sourceOutput)
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
                    BuildSpec = BuildSpec.FromSourceFilename("./infra-as-code/CicdInfraAsCode/src/assets/codebuild/app-docker-image-buildsec.yaml"), // <= path relative to the git repo root
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
                            { "BuildConfig", new BuildEnvironmentVariable { Value = settings.BuildConfiguration } }
                        },
                        ComputeType = settings.BuildInstanceSize
                    },
                    Role = new Role(this, "Docker-Image-build-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        RoleName = $"{settings.ScopeName}-Build-Docker-Image-Role",
                        ManagedPolicies = CdkExtensions.FromAwsManagedPolicies("AmazonEC2ContainerRegistryPowerUser")
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
                    BuildSpec = BuildSpec.FromSourceFilename("./infra-as-code/CicdInfraAsCode/src/assets/codebuild/deployment-env-infra-buildspec.yaml"), // <= path relative to the git repo root
                    Environment = new BuildEnvironment
                    {
                        Privileged = true,
                        BuildImage = LinuxBuildImage.UBUNTU_14_04_DOCKER_18_09_0,
                        ComputeType = settings.BuildInstanceSize,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>()
                        {
                            { "DbEngine", new BuildEnvironmentVariable { Value = settings.DbEngine.ToString() } },
                            { "BuildConfig", new BuildEnvironmentVariable { Value = settings.BuildConfiguration } },
                            { "DockerImageRepository", new BuildEnvironmentVariable { Value = settings.DockerImageRepository } }
                        }
                    },
                    Role = new Role(this, "App-deployment-env-build-role", new RoleProps
                    {   // Need to explicitly grant CodeBuild service permissions to let it push Docker 
                        // images to ECR, because we do `docker push` straight from the Build stage, bypassing
                        // Deploy stage, where it could have been done too.
                        AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                        RoleName = $"{settings.ScopeName}-Build-Deployment-Env-Role",
                        ManagedPolicies = CdkExtensions.FromAwsManagedPolicies(
                            //"CloudWatchLogsFullAccess", 
                            "AWSCodeDeployRoleForECSLimited",
                            "AWSCloudFormationFullAccess"
                        ) 
                    }),
                    Cache = Cache.Local(LocalCacheMode.SOURCE, LocalCacheMode.DOCKER_LAYER)
                })
            });

            return codeBuildAction;
        }

        private Repository CreateDockerRepo(UnicornStoreCiCdStackProps settings)
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
