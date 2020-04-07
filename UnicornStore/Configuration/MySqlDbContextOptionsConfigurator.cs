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