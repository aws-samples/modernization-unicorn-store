<!--
+++
title = "Explore CDK Stack Source"
menutitle = "Explore CDK Stack Source Code"
date = 2019-10-16T16:05:30-04:00
weight = 81
pre = "<b>7.8 </b>"
+++
-->
Now it's perfect time to take a look at how a CDK "stack" written in C# looks like.

For best experience, please *mark "ProdEnvInfraAsCode" as a Startup Project*, open "Program.cs" in the IDE editor, put the break on the line with the `new UnicornStoreFargateStack(...)" and start debugging. 

First you will skip over the straightforward-sounding `LoadConfiguration()` method that reads in "appSettings.json" etc. and passes loaded data to the `UnicornStoreFargateStack(...)` constructor in the `settings` variable.

Next, with the debugger, step into the "UnicornStoreFargateStack(...)" constructor. That's where the highest-level flow creating [Unicorn Store application hosting environment](./50-project-structure.html#architectural-diagram-of-the-application-hosting-environment) in its entirety, is defined. 

> Please use the debugger to step inside every method you can in this project - it will only take five minutes or so to do that, but it will provide more insight info what CDK is all about than most of the "Hello, World" type of examples. This project is still pretty small in terms of code line count, but it provisions really sophisticated hosting environment. 
> 
> The exercises of stepping through this project using debugger may be the most valuable activity of the entire Workshop.

For those not inclined to step-debug through the code, here's the screenshot of the main logic orchestrating application hosting environment creation at the highest level.

![CDK Stack in C# for the app Hosting Environment](./images/hosting-env-stack-source.png)

If you have experience with CloudFormation, you likely will appreciate ability to use imperative and object-oriented features of a real programming language, which may provision different resources depending on the execution path dictated by configuration settings.