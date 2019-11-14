<!--
+++
title = "Explore CDK Stack Source"
menutitle = "Explore CDK Stack Source Code"
date = 2019-10-16T16:05:30-04:00
weight = 81
pre = "<b>7.9 </b>"
+++
-->
Now it's perfect time to take a look at how a CDK "stack" written in C# looks like.

For best experience, please *mark "ProdEnvInfraAsCode" as a Startup Project*, open its "Program.cs" in the IDE editor, put a break on the line with the `new UnicornStoreFargateStack(...)" and start debugging. 

First you will skip over the straightforward-sounding `LoadConfiguration()` method that reads in "appSettings.json" etc. and passes loaded data to the `UnicornStoreFargateStack(...)` constructor in the `settings` variable.

Next, with the debugger, step into the "UnicornStoreFargateStack(...)" constructor. That's where the highest-level flow creating [Unicorn Store application hosting environment](./50-project-structure.html#architectural-diagram-of-the-application-hosting-environment) in its entirety is defined.

> Please use the debugger to step inside every method you can in this project - it will only take five minutes or so to do that, but it will provide more insight into what CDK is all about than most of the "Hello, World" type of examples. This project is still pretty small in terms of code line count, but it provisions really sophisticated hosting environment. 
> 
> The exercise of stepping through this project using debugger may be the most valuable activity of the entire Workshop.

For those not inclined to step-debug through the code, here's the screenshot of the main logic orchestrating application hosting environment creation at the highest level.

![CDK Stack in C# for the app Hosting Environment](./images/hosting-env-stack-source.png)

If you have experience with CloudFormation, you likely will appreciate ability to use imperative and object-oriented features of a real programming language, which may provision different resources depending on the execution path dictated by configuration settings.

### Cool CDK Feature Worth Noting: Packaging Runtime Dependencies

For many infra-as-code use cases, a CloudFormation template defining the infrastructure is not the only thing required. Very often additional run-time components are necessary. Lambda functions is likely a most common run-time dependency for a "stack". In our case, the final stage of the CI/CD pipeline employs a Lambda function, (which in reality is a small Node.js project) that runs to recycle the application and re-load it from the image containing latest changes. Lambda code, however, does not get embedded into the generated CloudFormation template - it lives outside and instead is referenced by the it.

The above means that we have a choice of keeping the Lambda function in a separate source code repository if we want to, but it makes much more sense to keep it within the CDK project. But how will one make generated CloudFormation template reference the Node.js artifact? If you used other infra-as-code tools, you may have run into a situation where you had to make your infra-as-code project aware of its own Git location to let is reference additional runtime dependency components from the same repo. To avoid this awkward self-referencing, CDK allows packaging of several types of artifacts right from the project source itself.

In this project, the `CreateLambdaForRestartingEcsApp()` method has this line:
```cs
Code = Code.FromAsset("assets/lambda/ecs-container-recycle")
```
The magic it performs allows keeping stack-specific lambda code right here in the same Git repository, and references this lambda in the way that avoids awkward self-referencing of the Git repo.

Lambdas is not the only type of external dependencies that can be embedded into the CDK project to package runtime artifacts and reference them from generated CloudFormation templates.