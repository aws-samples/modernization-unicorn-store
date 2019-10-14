<!--
+++
title = "Prerequisites"
date = 2019-10-11T17:39:00-04:00
weight = 20
+++
-->

> To successfully execute any module of this workshop, following **common** requirements need to be fulfilled, in addition to any module-specific requirements outlined in each module guide. 
> 
> If you are taking this lab within the scope of an *Amazon event*, in many cases a *remote VM with all necessary prerequisites will be made available* for workshop participants.

Note to *non-Windows users*: since most modules are based on .NET Core version of the application, Mac and Linux users should have no trouble executing most modules. Mac users should be able to use either [Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/installation?view=vsmac-2019), or [Jetbrains Rider](https://www.jetbrains.com/rider/download/#section=mac) for an IDE. Linux users should use [Jetbrains Rider](https://www.jetbrains.com/rider/download/#section=linux) IDE. Also, some of the links below target [Chocolatey](https://chocolatey.org/) - Windows package manager, Mac and Linux users could simply switch to Homebrew or yum/apt/etc package managers respectively.

(Although it is possible to use just `dotnet` CLI for this workshop, for simplicity and efficiency sake we dare to assume that takers of this 300-400 level workshop focusing on software development already know how to install their most basic tools, so that this workshop could skip IDE etc. installation steps boilerplate.)

### Software Required for Most Modules

* [.NET Core Framework 2.2](https://dotnet.microsoft.com/download).
* [Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/downloads/) or any other IDE capable of building and running Visual Studio solutions.
* An RDBMS like [MySQL](https://chocolatey.org/packages/mysql), [PostgreSQL](https://chocolatey.org/packages/postgresql) or MS [SQL Server LocalDB](https://chocolatey.org/packages/sqllocaldb), along with a corresponding management console, will be required for most modules.
* [AWS account](https://aws.amazon.com/premiumsupport/knowledge-center/create-and-activate-aws-account/) to run the lab in the AWS could.
* [AWS CLI](https://aws.amazon.com/cli/) to interact with AWS services from your system.
* [Node.js](https://nodejs.org/en/download/) and [AWS CDK](https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html) for building infrastructure-as-code projects that are part of the solution.
* [Git](https://chocolatey.org/packages/git) for pushing lab code to the cloud CI/CD pipeline for deployment.

The list above provides good baseline for your environment, but some modules will have additional requirements specified in the introduction section of each module.