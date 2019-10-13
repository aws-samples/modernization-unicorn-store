+++
title = "What's Inside"
date = 2019-10-11T17:38:34-04:00
weight = 10
+++

#### ASP.NET Application

The foundation of every module here is an ASP.NET *Core* Unicorn Store web application that has an MVC/Razor UI and MVC Controllers implementing business logic and data access layers. Unicorn Store application is based on an established ASP.NET Music Store sample and thus should be fairly familiar to many participants.

Data Access components of the application are written using *code-first* style, enabling easy switching of the DAL from using SQL Server to other RDBMS, including open-source databases like MySQL and PostgreSQL.

#### CI/CD Pipeline and Deployment Environment Infra As Code

All workshop modules feature AWS CI/CD pipeline infrastructure and deployment environment infrastructure as expressed as AWS CDK based C# code, thus enabling an easy building and deployment of the application in AWS cloud.