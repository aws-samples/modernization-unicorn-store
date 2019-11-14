<!--
+++
title = "Modifying Startup.cs"
date = 2019-10-15T17:13:32-04:00
weight = 71
pre = "<b>7.3 </b>"
+++
-->

1. Please open "Startup.cs" file of the "UnicornStore" project in Visual Studio.
2. Add following line to the top part of the file:
```cs
using MySql.Data.MySqlClient;
```

3. Find `ConfigureDatabaseEngine()` method and replace the "`#if POSTGRES`" line in it with
```cs
#if MYSQL
            this.HookupMySQL(services);
#elif POSTGRES
```
4. Now, to implement missing "HookupMySQL()" function, find the `HookupPostgres()` method and replace the "`#if POSTGRES`" line right above the function with
```cs
#if MYSQL
        private void HookupMySQL(IServiceCollection services)
        {
#if Debug || DEBUG
            // The line below is a compile-time debug feature for `docker build` outputting which database engine is hooked up 
#warning Using MySQL for a database
#endif
            this.HookupDatabase<MySqlConnectionStringBuilder, MySqlDbContextOptionsConfigurator>(services, "MySQL");
        }

#elif POSTGRES
```
"HookupMySQL()" method does two things:

* Adds compile-time output, producing a message that MySQL is the RDBMS type the project is compiled for.
* Registers MySQL database engine services with .NET Core IoC container (`IServiceCollection`), specifically it registers stock `MySqlConnectionStringBuilder` from the MySQL library added via NuGet, and proprietary `MySqlDbContextOptionsConfigurator` that we are about to implement on the next step. `HookupDatabase()` is a reusable method, which among a few others was written for this workshop with the aim of achieving [Database Freedom](https://aws.amazon.com/solutions/databasemigrations/database-freedom/), i.e. to break original application's dependency on SQL Server and make it compatible with wide array of open source database engines, which also makes the application compatible with [Amazon RDS](https://aws.amazon.com/rds/) and [Aurora](https://aws.amazon.com/aurora/) services.
