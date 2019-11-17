<!--
+++
title = "Prerequisites"
date = 2019-10-11T17:39:00-04:00
weight = 20
+++
-->

> To successfully execute any module of this workshop, following **common** requirements need to be fulfilled, in addition to any module-specific requirements outlined in each module guide. 
> 
> If you are taking this lab within the scope of an *Amazon event*, in many cases a *remote VM with all necessary prerequisites will be made available* to workshop participants, so *you may skip this chapter* altogether. (Mac users, to access remote Windows VM, please ensure you have [Remote Desktop Client](https://apps.apple.com/us/app/microsoft-remote-desktop-10/id1295203466) installed).

Note to *non-Windows users*: since most modules are based on .NET Core version of the application, Mac and Linux users should have no trouble executing most modules. Mac users should be able to use either [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/installation?view=vsmac-2019), or [Jetbrains Rider](https://www.jetbrains.com/rider/download/#section=mac) as an IDE. Linux users should use [Jetbrains Rider](https://www.jetbrains.com/rider/download/#section=linux) IDE. Also, some of the links below target [Chocolatey](https://chocolatey.org/) - Windows package manager. Mac and Linux users could simply switch to Homebrew or yum/apt/etc package managers respectively.

(Although it is possible to use just `dotnet` CLI for this workshop, for simplicity and efficiency sake we dare to assume that takers of this workshop focusing on software development already know how to install their most basic tools, so the workshop guide could skip IDE etc. installation steps boilerplate.)

### Software Required for Most Modules

* [.NET Core Framework 3.0](https://dotnet.microsoft.com/download).
* [Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/downloads/) or any other IDE capable of building and running Visual Studio solutions.
* An RDBMS like [MySQL](https://chocolatey.org/packages/mysql), [PostgreSQL](https://chocolatey.org/packages/postgresql) or MS [SQL Server LocalDB](https://chocolatey.org/packages/sqllocaldb), along with a corresponding management console, will be required for most modules.
* [AWS account](https://aws.amazon.com/premiumsupport/knowledge-center/create-and-activate-aws-account/) to run the lab in the AWS cloud.
* [AWS CLI](https://aws.amazon.com/cli/) to interact with AWS services from your system.
* [Git](https://chocolatey.org/packages/git) for pushing lab code to the cloud CI/CD pipeline for deployment.
* [Node.js](https://nodejs.org/en/download/) and [AWS CDK](https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html) for building infrastructure-as-code projects that are part of the solution, as well as for bootstrapping CI/CD pipeline infra into AWS cloud.
  
  > Please note that AWS SDK release cycle is very short as the team iterates on the service very aggressively, which results in rapid introduction of sometimes breaking changes that may not always follow [SemVer](https://semver.org/) guidelines. It's worth treating CDK minor version as major to minimize confusion. To get the **AWS CDK version** to install, check the content of the "./infra-as-code/CicdInfraAsCode/src/appsettings.json" file, from the Git repo root, and look for the value of the `CdkVersion` property.


The list above provides good *baseline* for required development environment. Some modules, however, will have additional requirements specified in the introduction section of each module.