<!--
+++
title = "MySQL Options Configurator"
date = 2019-10-15T23:10:12-04:00
weight = 73
pre = "<b>7.4 </b>"
+++
-->
Each database engine supported by the Unicorn Store application needs a `DbContextOptionsConfigurator` subclass. "DbContextOptionsConfigurator" facilitates deferred and dynamic update of stored database connection information in case when connection string information stored in the "appsettings.json" file changes while the application is running.

> Unlike common practice shown in countless .NET Core samples, loading connection string once when `Configure()` method is called goes against the design goals of the `IConfiguration`-based settings handling, as doing so circumvents dynamic settings reloading capabilities of "IConfiguration".
> 
> To work-around this somewhat deeply ingrained anti-pattern, a small set of classes was created to enable deferred loading and dynamic re-loading database connection information from the "appsettings.json", without having to restart the application.

1. In the IDE, please expand the "UnicornStore" project tree, right-click the `Configuration` folder, and select "Add | Class" from the context menu.
2. For class name, enter `MySqlDbContextOptionsConfigurator.cs` and hit Enter.
3. Replace generated stock content of the file with
```cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace UnicornStore.Configuration
{
    public class MySqlDbContextOptionsConfigurator : DbContextOptionsConfigurator
    {
        public MySqlDbContextOptionsConfigurator(
            DbConnectionStringBuilder dbConnectionStringBuilder,
            ILogger<MySqlDbContextOptionsConfigurator> logger
            ) : base(dbConnectionStringBuilder, logger)
        {
        }

        public override string DbEngine => "MySQL";

        internal override void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            base.Configure(optionsBuilder);

            optionsBuilder.UseMySql(this.dbConnectionStringBuilder.ConnectionString);
        }
    }
}
```

As you have probably noticed, the only consequential line of code in the snippet above is
```cs
optionsBuilder.UseMySql(this.dbConnectionStringBuilder.ConnectionString);
```
This line is the one usually called from the "Startup.cs". Here the only thing we do differently is we defer calling `UseMySql()` to when "IConfiguration" becomes injectable, like you do it when you need "IConfiguration" in your MVC controller. 

> Injecting "IConfiguration", as it's done in a typical MVC controller example, is the right way to use configuration settings. Manually loading connection string data, as often done in "Startup.cs", *breaks dynamic settings reload support*. Mixing two mutually-exclusive approaches makes no practical sense, so we took this opportunity to make configuration settings data usage consistent across the entire ASP.NET application. Feel free to improve this logic and create a PR.

