<!--
+++
title = "MySQL in CI/CD CDK Project"
date = 2019-10-16T01:18:54-04:00
weight = 79
pre = "<b>7.7 </b>"
+++
-->
> Here we'll add MySQL support to the CI/CD CDK project.

1. Please start with marking "CicdInfraAsCode" project as a "Startup Project" in Visual Studio.
2. Open "`UnicornStoreCiCdStackProps.cs`" file in the IDE editor.
3. Find the `DbEngineType` enum and add `MySQL` entry to it.
4. Find the `DbEngine` property and replace the `#if POSTGRES` line with
```cs
#if MYSQL
            DbEngineType.MySQL;
#elif POSTGRES
```
5. Run the project to ensure there are no exceptions thrown.
6. Open Command Prompt window and *cd* to the root of the "CicdInfraAsCode" project.
7. Run `cdk diff` - command that compares currently existing the infrastructure (of the CI/CD pipeline in this case) created by the infra-as-code "stack" earlier, with the update infrastructure. You should see output looking like
```
Synthesized to "cdk.out".
Stack Unicorn-Store-CI-CD-PipelineStack
The Unicorn-Store-CI-CD-PipelineStack stack uses assets, which are currently not accounted for in the diff output! See https://github.com/aws/aws-cdk/issues/395
Resources
[~] AWS::CodeBuild::Project CodeBuildProject CodeBuildProject4B91CF3F
 └─ [~] Environment
     └─ [~] .EnvironmentVariables:
         └─ @@ -54,7 +54,7 @@
            [ ] {
            [ ]   "Name": "DbEngine",
            [ ]   "Type": "PLAINTEXT",
            [-]   "Value": "Postgres"
            [+]   "Value": "MySQL"
            [ ] },
            [ ] {
            [ ]   "Name": "BuildConfig",
            @@ -64,6 +64,6 @@
            [ ]   {
            [ ]     "Name": "ImageTag",
            [ ]     "Type": "PLAINTEXT",
            [-]     "Value": "Postgres"
            [+]     "Value": "MySQL"
            [ ]   }
            [ ] ]
[~] AWS::CodeBuild::Project DeploymentEnvCreationProject DeploymentEnvCreationProject70D82E6D
 └─ [~] Environment
     └─ [~] .EnvironmentVariables:
         └─ @@ -2,7 +2,7 @@
            [ ] {
            [ ]   "Name": "DbEngine",
            [ ]   "Type": "PLAINTEXT",
            [-]   "Value": "Postgres"
            [+]   "Value": "MySQL"
            [ ] },
```
Please note parts reflecting the RDBMS type change in the output above:
```
            [-]   "Value": "Postgres"
            [+]   "Value": "MySQL"
```
> `cdk diff` is a very, very useful tool, allowing a developer to build-out an infrastructure as a base line, and then modify the infra-as-code project and run "cdk diff" to see what will be changed if they run "cdk deploy", providing *fast and non-destructive way of understanding what changes in the the infrastructure will be made by corresponding changes in the CDK project*.

8. Run `cdk deploy --require-approval never` and it should complete in a minute or two without errors, updating the CI/CD pipeline infrastructure in AWS with MySQL configured and the database engine.
9. Now we need to trigger the pipeline by committing and pushing code changes to the AWS CodeCommit. For that please run `git commit` followed by `git push aws` and in a minute or so you should see the [CI/CD pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) executing.

> Since hosting environment "stack" will now need to replace PostgreSQL database with MySQL database, it will again take roughly 15 minutes to complete the transition. Please keep monitoring the state of [the pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home), and once it finishes, please verify everything is well by using [these steps](./63-verify-ci-cd-completion.html#verifying-the-ci-cd-pipeline-run-has-completed).