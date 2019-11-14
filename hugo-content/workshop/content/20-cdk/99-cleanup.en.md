<!--
+++
title = "Cleanup"
date = 2019-10-15T11:53:46-04:00
weight = 999
+++
-->

Some manual clean up steps are required to delete resources created in the course of this module. You may need to follow these steps if you *used your personal or corporate AWS Account*. 

You will also need to use these steps if you are taking this lab at an AWS event and you simply want to reset your cloud resources and *restart the module from the [beginning](./10-overview.html)*.

> When using your personal or corporate AWS Account, leaving resources created by this lab will result in recurring monthly charges for used resources.
> 
> Commands below may remove most of the resources (no guarantees though) created in the course of this module.

### Delete Infrastructure "Stacks" Created by CDK Projects

To see how CDK could be leveraged for cleanup, open a Command Prompt window and cd to the root of the "CicdInfraAsCode" project. Here run 
```bash
cdk destroy
```
to effectively delete the CloudFormation stack produced by the project, which in turn destroys AWS resources comprising our CI/CD pipeline.

As an alternative, you may run 
```bash
aws cloudformation delete-stack --stack-name Unicorn-Store-CI-CD-PipelineStack
```

Since hosting environment stack was run as part of the CI/CD pipeline, deleting it is easily done with deleting the corresponding CloudFormation stack:
```bash
aws cloudformation delete-stack --stack-name UnicornSuperstoreStack
```
Although both commands return instantaneously, the process of deleting resources up to 10 minutes, give or take. To see the progress, please browse to [CloudFormation -> Stacks](https://console.aws.amazon.com/cloudformation/home) in AWS Console. There you should see **DELETE_IN_PROGRESS** status next to "UnicornSuperstoreStack" and "Unicorn-Store-CI-CD-PipelineStack" items. Refresh the page periodically using the round arrow icon.

### Delete ECR Docker Image Repository

```bash
aws ecr delete-repository --repository-name unicorn-store-app --force
```
