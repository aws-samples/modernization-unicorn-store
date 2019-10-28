<!--
+++
title = "What's Inside"
date = 2019-10-11T17:38:34-04:00
weight = 10
+++
-->

#### ASP.NET Application

The foundation of every module here is an ASP.NET *Core* Unicorn Store web application. It is based on a well-worn [ASP.NET Music Store sample app](https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/mvc-music-store/mvc-music-store-part-1) and thus should be fairly familiar to many participants.

Unicorn Store is has design that should make most .NET developers very comfortable. It has MVC/Razor UI and MVC Controllers implementing business logic. Data Access components of the application employ Entity Framework Core and are written using *code-first* style, enabling easy switching of the DAL from using SQL Server to other RDBMS, including open-source databases like MySQL and PostgreSQL.

#### CI/CD Pipeline and Deployment Environment Infra As Code

All workshop modules feature AWS CI/CD pipeline infrastructure and deployment environment infrastructure expressed as AWS CDK based C# code, thus enabling an easy building and deployment of the application in AWS cloud.