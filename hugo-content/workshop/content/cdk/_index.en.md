+++
title = "CDK, CI/CD and Infra-as-Code"
menuTitle = "CDK, CI/CD and Infra asCode"
date = 2019-10-10T16:56:01-04:00
weight = 10
chapter = true
pre = "<b>2. </b>"
+++

This module covers several facets of a modern application, namely creation of a cloud-based CI/CD pipeline, as well as application cloud deployment environment, using AWS CDK based infrastructure-as-code.

The end product of this module is two Visual Studio C# projects in the Solution, each responsible for generation AWS CloudFormation templates that a) create AWS CodePipeline based CI/CD pipeline that builds and deploys the application, and b) create application deployment environment in the AWS cloud, consisting of Amazon Elastic Container Service (ECS Fargate) and Amazon Relational Database Service (RDS) hosting a selection of popular relational databases like Aurora MySQL (HA), Aurora Postgres (HA), and SQL Server.