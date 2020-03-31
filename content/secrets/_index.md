# Securing Your .NET Container Secrets

 As customers move .NET workloads to the cloud, many start to consider containerizing their applications because of the agility and cost savings that containers provide. Combine those compelling drivers with the multi-OS capabilities that come with .NET Core, and customers have an exciting reason to migrate their applications. A primary question is how they can safely store secrets and sensitive configuration values in containerized workloads. In this workshop, learn how to safely containerize the Unicorn Store.

You will learn how to run the Unicorn Store which is an ASP.NET Core application in a Docker container while connecting to a SQL backend (Database=UnicornStore) in [Amazon RDS](https://aws.amazon.com/rds/). The RDS credentials to the database in are stored in [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/) along with other sensitive information needed for the application to run. This allows the Unicorn Store application to safely connect to the database from the container without storing the secrets in a file on the container or in source control.

Take the time to read [mutiple environments in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-3.0) and [safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows) before starting so you understand the various different configuration options.

## Architecture Overview

![Unicorn Store Architecture with AWS Secrets Manager and Amazon RDS](/static/images/secrets/secrets-manager-architecture.png)

## Getting Started

Click [here](/content/secrets/prerequisites/_index.md) to start the workshop.