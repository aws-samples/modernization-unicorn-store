<!--
+++
title = "Testing CI/CD Changes"
menutitle = "Testing CI/CD Project Changes"
date = 2019-10-30T14:13:48-04:00
weight = 80
pre = "<b>7.8 </b>"
+++
-->

1. Run the CicdInfraAsCode project from the IDE to ensure there are no exceptions thrown.
2. Open Command Prompt window and *cd* to the root of the "CicdInfraAsCode" project.
3. Run `cdk diff` - command that compares currently-provisioned AWS infrastructure (of the CI/CD pipeline in this case) and created earlier by the infra-as-code "stack", with the infrastructure modified by the changes to the project codebase. You should see output looking like
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
> `cdk diff` is a very, very useful command, allowing developers to build-out an infrastructure base line, and then make changes to the infra-as-code project code and run "cdk diff" to see what will be changed in the cloud infrastructure if they run "cdk deploy". That provides *fast and non-destructive way of understanding what changes in the the infra-as-code project will be made by corresponding changes in the CDK project*.

4. Run "`cdk deploy --require-approval never`" and it should complete in a minute or two without errors, updating the CI/CD pipeline infrastructure in AWS with MySQL configured as the database engine.
5. Now we need to *trigger the pipeline* by committing and pushing code changes to the AWS CodeCommit. For that please run following commands *from the root directory of the Solution*:
```bash
git add .
git commit -m "Added MySQL support"
git push aws
```
 In a few moments you should see the [CI/CD pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) running.

> Since hosting environment "stack" will now need to replace PostgreSQL database with MySQL database, it will again take roughly 15 minutes to complete the transition. Please keep monitoring the state of [the pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home), and once it finishes, please verify everything is well by using [these steps](./63-verify-ci-cd-completion.html#verifying-the-ci-cd-pipeline-run-has-completed).

> Another important point: switching from Postgres to MySQL does not transfer data from Postgres to MySQL. All *data stored in the Postgres database will be lost*, and an *empty MySQL database will be crated*.