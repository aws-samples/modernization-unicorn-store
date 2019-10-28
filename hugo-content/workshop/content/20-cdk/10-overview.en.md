<!--
+++
title = "CDK Module Overview"
date = 2019-10-12T18:06:59-04:00
weight = 10
+++
-->
Welcome to the `.NET AWS CDK` module of the workshop!

### How to Run This Site Locally

Copy commands below from [https://tinyurl.com/dotnet-cdk-lab](https://tinyurl.com/dotnet-cdk-lab). Git should be installed as a pre-requisite.

```bash
git clone https://github.com/vgribok/modernization-unicorn-store.git
cd modernization-unicorn-store
git checkout cdk-module
./docs/en/index.html
```

### Workshop Goals

The goal of this lab is to guide participants through:

1. Running an existing .NET CDK project locally to bootstrap application's CI/CD pipeline infrastructure in AWS.
2. Adding support for MySQL database to both ASP.NET application codebase, as well as to the existing infra-as-code C# CDK projects (one defining cloud CI/CD pipeline and another defining cloud application deployment environment).
3. Creating app hosting cloud infrastructure in AWS by running hosting infra .NET CDK project from within the CI/CD pipeline.
4. Stepping in debugger through the code of two .NET CDK projects to explore how a CDK project that is a couple notches more complex than a "[Hello, World](https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html)" looks like.  

This workshop *skips the part showing basics of creating a new CDK project* and adding a few basic CDK Constructs, and instead focuses on somewhat more in-depth capabilities of the CDK, by providing higher-fidelity code samples implementing closer-to-real-life scenarios. This means that the lab will start with a couple of existing, but still pretty small CDK projects, and the lab flow focuses on modifying these existing projects rather creating new ones.

The aim of the lab is to help you learn how to take your cloud-unaware ASP.NET Core application and use `C#` to write code defining:

1. **CI/CD pipeline** infrastructure in AWS cloud that builds and deploys the application.
2. AWS cloud **application deployment infrastructure**, including an application hosting components and a database: Amazon Elastic Container Service (`ECS` Fargate) and Amazon Relational Database Service (`RDS`), respectively. RDS hosts a selection of popular relational databases like Aurora MySQL (HA), Aurora Postgres (HA), and SQL Server.

> If you find yourself struggling with the lab or running into unexpected errors, you may skip ahead by checking out `cdk-module-completed` Git branch, where all changes required for adding MySQL support are already implemented.

### CKD Demystified

> [AWS Cloud Development Kit](https://docs.aws.amazon.com/cdk/latest/guide/home.html) is a set of higher-level abstraction components built on top of the Amazon CloudFormation - an indispensible previous-generation infrastructure-as-code service, with the major difference  that CDK lets programmers use most of their favorite programming languages, like C#, to generate CloudFormation templates while writing *order of magnitude less code* than with CloudFormation.

CDK consists of a CLI and a set of libraries available for most popular programming languages. In the case of .NET CDK, the libraries are added via [NuGet](https://www.nuget.org/packages/Amazon.CDK/).

A .NET CDK project is a Console app, generating AWS CloudFormation template. CDK CLI is a convenience tool making it possible to bypass direct contact with lower-level CloudFormation templates and related commands of AWS CLI.
