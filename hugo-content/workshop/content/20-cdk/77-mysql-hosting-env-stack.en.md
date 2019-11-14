<!--
+++
title = "Adding MySQL Support to the Hosting Env CDK Project"
menutitle = "MySQL in Hosting Env CDK Proj"
date = 2019-10-16T00:15:05-04:00
weight = 77
pre = "<b>7.6 </b>"
+++
-->
> Next couple of sections will guide you through adding MySQL support to the `ProdEnvInfraAsCode` project that defines *application cloud hosting environment* using infrastructure-as-code approach.
>
> MySQL database underpinning Unicorn Store application is going to run in AWS cloud and is implemented either by [Amazon RDS for MySQL](https://aws.amazon.com/rds/mysql/) or by [Amazon Aurora](https://aws.amazon.com/rds/aurora/) service, depending whether the infrastructure is configured to run in a single-instance or in the cluster mode.
> 
> Amazon RDS and Amazon Aurora are SQL Server, MySQL and PostgreSQL-compatible, managed, "serverless" services, capable of replacing self-managed RDBMS deployments running on unmanaged VMs or in containers. (AWS Aurora does not support SQL Server - only AWS RDS does).

Please start by marking "ProdEnvInfraAsCode" project inside the "infra-as-code" Visual Studio folder as a "*Startup Project*".

#### Updating UnicornStoreDeploymentEnvStackProps.cs

"UnicornStoreDeploymentEnvStackProps.cs" file holds configuration properties used by the hosting environment CDK infra-as-code "stack". We need to make a few changes here.

1. Open the "UnicornStoreDeploymentEnvStackProps.cs" file in the IDE editor.
2. Find the `DbEngineType` enum and add `MySQL` entry to the enum.
3. Find the `DbEngine` property and replace the "`#if POSTGRES`" line with
```cs
#if MYSQL
            DbEngineType.MySQL;
#elif POSTGRES

```
4. Find the `CreateDbConstructFactory()` method and add the following to the top of the `switch` statement:
```cs
                case DbEngineType.MySQL:
                    return new MySqlConstructFactory(this);
```
5. At this point the only error on this page will be undefined `MySqlConstructFactory()` constructor from the class that will be implemented in the section below.

#### Implementing MySqlConstructFactory Class

1. Right-click `Reusable` folder under the "ProdEnvInfraAsCode" project in Visual Studio and select "Add | Class" from the context menu.
2. For the class name, enter `MySqlConstructFactory.cs` and hit Enter.
3. Replace generated stock implementation with 
```cs
using Amazon.CDK.AWS.RDS;

namespace ProdEnvInfraAsCode.Reusable
{
    public class MySqlConstructFactory : DatabaseConstructFactory
    {
        public MySqlConstructFactory(UnicornStoreDeploymentEnvStackProps settings)
            :base(settings)
        {
        }

        protected override DatabaseInstanceEngine DbInstanceEgnine => DatabaseInstanceEngine.MYSQL;

        protected override DatabaseClusterEngine DbClusterEgnine => 
            base.DbClusterEgnine ?? DatabaseClusterEngine.AURORA_MYSQL;

        protected override string ExistingAuroraDbParameterGroupName => "default.aurora-mysql5.7";

        internal override string DBConnStrBuilderUserPropName => "UserID";
    }
}
```
4. This class above supplies four settings for a handful of AWS RDS infrastructure settings:
   * `DatabaseInstanceEngine.MYSQL` for the database instance engine, i.e. for when RDS service runs in a single-instance mode, as opposed to the cluster mode.
   * `DatabaseClusterEngine.AURORA_MYSQL` for the database cluster engine, i.e. when RDS service runs in a cluster mode, and not in the single-instance mode.
   * "`default.aurora-mysql5.7`" as a name of setting set known as a [Parameter Group](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_WorkingWithParamGroups.html). Parameter Groups are maintained by AWS and are organized in a handful of pre-defined, named parameter sets made available to infrastructure builders. Here we select a most common, default set of parameters for the RDS MySQL.
   * "`UserID`" here is the name of the property defining database user/login name in the Connection String Builder subclass for a specific database engine type. Here it means that for MySQL, Connection String Builder property carrying database username is called "UserID".

> As you can see, "MySqlConstructFactory" pretty much in its entirety simply serves up a few overrides to the "DatabaseConstructFactory" base class, driving a few changes in how Unicorn Store database in AWS RDS is going to be configured.

#### Verifying ProdEnvInfraAsCode Project

Run the "ProdEnvInfraAsCode" project an ensure it did not throw any exceptions. The output is the CloudFormation template that can be used to provision Unicorn Store application hosting infrastructure.