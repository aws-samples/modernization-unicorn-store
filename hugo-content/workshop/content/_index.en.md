+++
title = "ASP.NET Modernization Workshop"
date = 2019-10-10T10:57:53-04:00
weight = 10
+++

This site comprises a set of workshop modules created to showcase how an ASP.NET application can be converted from being IIS and SQL Server bound monolith into a modern cross-platform application capable of running in the cloud.

The foundation of every lab here is an ASP.NET *Core* Unicorn Store web application that has an MVC/Razor UI and MVC Controllers implementing business logic and data access layers. The application is based on an established ASP.NET Music Store sample. Unicorn Store, unlike its predecessor, can use multiple database engines: SQL Server, MySQL and PostgreSQL, with a simple change in selected build configuration. The application will have *only open-source dependencies* in most workshop modules, enabling the application to run either locally or on any provider's cloud with no changes.

All workshop modules will have a cloud CI/CD pipeline infrastructure and  deployment environment infrastructure as code, thus enabling an easy building and deployment of the application in AWS.