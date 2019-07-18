using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace UnicornStore.Configuration
{
    /// <summary>
    /// Defers resolution of the connection string configuration to after DI/IoC container 
    /// is built, enabling connection string configuration settings to be changed after 
    /// the application start, without needing application restart to notice changed 
    /// connection string configuration settings.
    /// </summary>
    public abstract class DbContextOptionsConfigurator
    {
        protected readonly DbConnectionStringBuilder dbConnectionStringBuilder;

        protected readonly ILogger logger;

        /// <summary>
        /// Instantiated by the DI container
        /// </summary>
        /// <param name="dbConnectionStringBuilder"></param>
        public DbContextOptionsConfigurator(
            DbConnectionStringBuilder dbConnectionStringBuilder,
            ILogger logger)
        {
            this.dbConnectionStringBuilder = dbConnectionStringBuilder;
            this.logger = logger;
        }

        /// <summary>
        /// Override in subclasses to specify a database engine, like
        /// "optionsBuilder.UseSqlServer(this.dbConnectionStringBuilder.ConnectionString)"
        /// </summary>
        /// <param name="optionsBuilder"></param>
        internal virtual void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            this.logger.LogInformation("Database Connection String: {ConnectionString}", this.DisplayConnectionString);
        }

        /// <summary>
        /// Log-friendly connection string, obfuscating passwords and other sensitive fields.
        /// </summary>
        public string DisplayConnectionString {
            get
            {
                string[] strikeoutConStrParams = { "user", "user id", "username", "uid", "password", "pwd", "secret" };

                IEnumerable<string> pairs = from key in this.dbConnectionStringBuilder.Keys.Cast<string>()
                                                     let pair = new
                                                     {
                                                         Key = key,
                                                         Value = strikeoutConStrParams.Contains(key.ToLowerInvariant()) ?
                                                                                    "*************" : this.dbConnectionStringBuilder[key]
                                                     }
                                                     select $"{pair.Key}={pair.Value}";
                return string.Join(";", pairs);
            }
        }
    }
}