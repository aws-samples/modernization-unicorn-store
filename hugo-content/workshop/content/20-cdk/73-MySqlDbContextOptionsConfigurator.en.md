<!--
+++
title = "MySQL Options Configurator"
date = 2019-10-15T23:10:12-04:00
weight = 73
pre = "<b>7.4 </b>"
+++
-->
Each database engine supported by the Unicorn Store application needs a `DbContextOptionsConfigurator` subclass. "DbContextOptionsConfigurator" facilitates deferred and dynamic updating of stored database connection settings in case when connection string information stored in the "appsettings.json" file changes while the application is running.

> "DbContextOptionsConfigurator" rectifies common anti-pattern that is unfortunately used in many ASP.NET Core sample apps, which load and cache database connection string settings only once per app lifetime during the `Configure()` method execution, going against the design goals of the `IConfiguration`-based settings handling that enable dynamic app settings reloading without requiring restarting of the app.
> 
> To work-around this somewhat deeply ingrained anti-pattern, a small set of classes was created for this workshop, "DbContextOptionsConfigurator" being the main class.

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
This line is the one usually called from the "Startup.cs". Here the only thing we do differently is we defer calling `UseMySql()` to after "IConfiguration" become injectable, like you do it when you need "IConfiguration" in your MVC controller. 

> Injecting "IConfiguration", as it's done in a typical MVC controller example, is the right way to use configuration settings. Manually loading connection string data, as often done in "Startup.cs", *breaks dynamic settings reload support*. Mixing two mutually-exclusive approaches makes no practical sense, so we took this opportunity to make configuration settings data usage consistent across the entire ASP.NET application. Feel free to improve this logic and create a PR.

