<!--
+++
title = "Starting Pipeline Build"
date = 2019-10-15T11:34:12-04:00
weight = 60
pre = "<b>5. </b>"
+++
-->

At this point the CI/CD pipeline cloud infrastructure, the build out of which was [launched](./40-creating-ci-cd-pipeline.en.md#launching-ci-cd-pipeline-infrastructure-creation) two chapters back, should now be completed.

First, please check the command prompt console where you ran `cdk deploy` for the "CicdInfraAsCode" project, to make there were errors. You should see something like the following at the tail of the output after command has finished:
```
 30/30 | 1:10:43 PM | CREATE_COMPLETE      | AWS::CloudFormation::Stack  | Unicorn-Store-CI-CD-PipelineStack

 ✅  Unicorn-Store-CI-CD-PipelineStack

Stack ARN:
arn:aws:cloudformation:us-east-1:123456789012:stack/Unicorn-Store-CI-CD-PipelineStack/eee6e58c-6808-48a2-9885-c719f44dd8b6
```

Next, navigate to the [CodePipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) in the AWS Console and observe "Unicorn-Store-CI-CD-Pipeline" in the list, showing the "Failed" status next to it. The pipeline is in the failed state because the source code of the Unicorn Store solution has not yet been pushed to the CodeCommit Git repository.

Please click on the "Unicorn-Store-CI-CD-Pipeline" link to see the never-run pipeline:
![CodePipeline in Failed state without source code](./images/pipeline-failed-no-source.png)

### Starting the Pipeline: Pushing Source Code to CodeCommit

To trigger the build and deployment pipeline, all we need to do is to push the Unicorn Store to the CodeCommit Git repository just created.

> Following step - adding Git "remote" (alias) pointing to the CodeCommit repository, can be skipped if you are taking this lab at an AWS event, as the "aws" Git remote is already on your dev VM.

Windows users, please use `Powershell` in *Administrator mode* for running commands that follow.

1. First, cd into the *root* of the Unicorn Store solution.
1. Create Git "remote" named "aws":

```ps
git remote add aws (aws codecommit get-repository --repository-name Unicorn-Store-Sample-Git-Repo | jq -r .repositoryMetadata.cloneUrlHttp)
```

1. Push the code to the CodeCommit repository:
```sh
git push aws cdk-module
```

1. Observe CodePipeline Build in-progress by waiting half a minute or so and  then going to the [CodePipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) page in the AWS Console:
![CodePipeline](./images/pipeline-in-progress.png)

> First run of the pipeline is likely to take approximately `15 minutes`, primarily due to time required to provision application database and ECS-based application hosting infrastructure. Subsequent pipeline runs cas run to completion in *under one minute*.

While the pipeline is busy building the application and provisioning application hosting infrastructure, we have about 15-20 minutes to start hacking the app and CDK project, adding MySQL support to it. Feel free to go to the next chapter and come back here later to ensure the pipeline 

### Verifying the CI/CD Pipeline Run Has Completed 

Whenever [the pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) has finished, feel free to browse to the [AWS Load Balancer](https://console.aws.amazon.com/ec2/v2/home#LoadBalancers:sort=loadBalancerName) page in the AWS Console, select a load balancer with the name starting with "Unico" and click the "copy to clipboard" icon next to the "**DNS Name**" field.

Then paste the URL into a new browser tab, hit Enter and observe Unicorn Application home page loaded:
![Unicorn Store application in browser](./images/unicorn-store-app-in-browser.png) 

Please follow these [verification steps](./30-running-app-locally.html#exploring-and-testing-unicorn-store-web-application-functionality) to ensure application functionality is not broken.
