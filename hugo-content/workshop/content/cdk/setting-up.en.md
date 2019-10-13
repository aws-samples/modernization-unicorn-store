+++
title = "Setting Up Dev Environment"
date = 2019-10-13T14:29:41-04:00
weight = 20
pre = "<b>1. </b>"
+++

You may *skip this page* and move on to the next step and you are *using Amazon-supplied VM image* (AMI) at an Amazon event, with all tools installed and pre-configured.

#### Pre-Requisites

* Either MS SQL Server ([LocalDB](https://chocolatey.org/packages/sqllocaldb) version is perfectly suitable), or PostgreSQL, per [Common Prerequisites](../intro/prerequisites.html).
* [MySQL](https://chocolatey.org/packages/mysql) is required as the goal of the module is to add MySQL support to the app. [MySQL Workbench](https://chocolatey.org/packages/mysql.workbench), a MySQL management UI console, is recommended.

#### Checking Out Source Code

In a directory of your choice, please run 
```bash
git clone https://github.com/vgribok/modernization-unicorn-store.git
git checkout cdk-module
```

#### Building and Running Unicorn Store Application Locally

* Open Visual Studio solution:
```bash
cd modernization-unicorn-store
UnicornStore.sln
```
* In VS Solution Explorer, click the Collapse All [-] icon to clean up the tree view and select UnicornStore.csproj as a Startup Project.
* Choose build configuration for your choice of RDBMS. You can use either MS SQL Server or PostgreSQL as a database at this point in the lab flow. (MySQL is not yet available at this point as it's a new RDBMS to add support for as a part of this lab.) When using  SQL Server *LocalDb*, no application configuration setting changes are necessary. You may simply select "DebugSqlServer" build configuration from the drop-down and run the application. For PostgreSQL, please select "DebugPostgres" from the build configuration drop down in the IDE.

TBD: PG user secret settings