<!--
+++
title = "Setting Up Dev Environment"
date = 2019-10-13T14:29:41-04:00
weight = 20
pre = "<b>1. </b>"
+++
-->

> If you are *using Amazon-supplied VM image* (AMI) at an Amazon event, with all tools installed and pre-configured for you, we recommend reading through this chapter without making any changes mentioned on this page.

#### Pre-Requisites

* Either MS SQL Server ([LocalDB](https://chocolatey.org/packages/sqllocaldb) version is perfectly suitable), or PostgreSQL, per [Common Prerequisites](../10-intro/20-prerequisites.html).
* [MySQL](https://chocolatey.org/packages/mysql) is required, as the goal of the module is to add MySQL support to the app. [MySQL Workbench](https://chocolatey.org/packages/mysql.workbench), a MySQL management UI console, is recommended.
* AWS CLI needs to be [configured](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-configure.html) to point to the desired AWS Region.

> For Windows Users Only:

1. [Chocolatey](https://chocolatey.org/docs/installation#install-with-cmdexe) package manager.
2. The [jq](https://chocolatey.org/packages/jq) utility that parses JSON.


#### Checking Out Source Code

In a directory of your choice, please run 
```bash
git clone https://github.com/vgribok/modernization-unicorn-store.git
git checkout cdk-module
```
The "`cdk-module`" git branch is where the sample app codebase lives.

#### Building and Running Unicorn Store Application Locally

* Open Visual Studio solution:
```bash
cd modernization-unicorn-store
UnicornStore.sln
```
* In VS Solution Explorer, click the Collapse All [-] icon to clean up the tree view and select UnicornStore.csproj as a Startup Project.
* Choose build configuration for your choice of RDBMS:

| Build Configurations                                                              | Notes |
| --------------------------------------------------------------------------------- | ----- |
| ![VS Build Configurations](images/solution-build-configurations.png?width=1000px) |  You can use either MS SQL Server or PostgreSQL as a database at this point in the lab flow. (MySQL is not yet available at this point as it's a new RDBMS to add support for as a part of this lab.) When using  SQL Server *LocalDb*, no application configuration setting changes are necessary. You may simply select "DebugSqlServer" build configuration from the drop-down and run the application. For PostgreSQL, please select "DebugPostgres" from the build configuration drop down in the IDE.|

#### Application Configuration for PostgreSQL 

You may skip this section if you plan to use MS SQL Server as a RDBMS for the application.

If you choose PostgreSQL as application's RDBMS, additional changes in application settings are needed for the application to connect to a database other than local SQL Server with integrated authentication.

| Notes | Secret Manger |
| ----- | ------------- |
| Please open UnicornStore project [Secret Manager](../10-intro/30-dotnet-secrets.html) by right-clicking on the project and selecting "`Manage User Secrets`" menu item. It will open `secrets.json` file in Visual Studio, where you can make changes as it was the `appsettings.json` file. <br/><br/> Changes made in "secrets.json" override values stored in the "appsettings.json" files at run time (when application runs locally), without making any changes to the "appsettings.json". | ![VS Project Secret Manager Menu](images/open-project-secret-manager.png?width=1100) |

Application setting named `UnicornDbConnectionStringBuilder` represents properties from RDBMS-specific subclasses of the [DbConnectionStringBuilder](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnectionstringbuilder?view=netcore-2.2). Since the plan here is to use PostgreSQL, properties of the "UnicornDbConnectionStringBuilder" section will represent properties of the [NpgsqlConnectionStringBuilder](https://www.npgsql.org/doc/api/Npgsql.NpgsqlConnectionStringBuilder.html#properties) class.

```json
{
  "DefaultAdminUsername": "admin@acme.com",
  "DefaultAdminPassword": "Passw0rd!",

   "UnicornDbConnectionStringBuilder": {
    "Password": "<YOUR POSTGRES PASSWORD>",
  }
}
 ```
 Please replace the content of the "secrets.js" file open in the Visual Studio, with the code snippet above, and replace the value of the `Password` property with your local Postgres password. Don't forget to save the file before moving on to the next section of the lab.

 > The values of "`DefaultAdminUsername`" and "`DefaultAdminPassword`" properties are used to login to the web application - that's root administrator credentials. Feel free to put any username/password you find easy to remember when you will be running the application later in the course of this workshop.