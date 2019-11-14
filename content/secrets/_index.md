# Secrets Introduction

When designing an application, one of the primary considerations is how sensitive data will be stored. A very common use-case is ensuring that something like the password and information needed to connect to your database aren’t written to a file or checked into a source control repository. This information should be considered sensitive and should only be accessed by the users and applications in a least privileged model. 

In this section, we will understand how to how to leverage [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/) which helps you protect secrets that are needed for your applications and makes it easy to manage and things like database credentials to name a few. 

We will also cover the concept of using a .NET Core tool called [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows) which allows you to store sensitive data during the development of your application. AWS Secrets Manager and .NET Core Secret Manager should never be confused but we will show you how to use both when designing your .NET Core application. By leveraging the .NET Core Secret Manager tool, a developer can easily create key/value objects in a JSON file on their local machine outside of the actual project to ensure the actual secrets aren’t checked into a source control repository.

Click [**here**](/content/secrets/secrets.md) to move to the next section where we will explain the basics of the ASP.NET Core Secret Manager tool and create the UNICORNSTORE_DBSECRET.
