using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UnicornStore.Configuration
{
    public class NpgsqlDbContextOptionsConfigurator : DbContextOptionsConfigurator
    {
        public NpgsqlDbContextOptionsConfigurator(
            DbConnectionStringBuilder dbConnectionStringBuilder,
            ILogger<NpgsqlDbContextOptionsConfigurator> logger
            ) 
            : base(dbConnectionStringBuilder, logger)
        {
        }

        internal override void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            base.Configure(optionsBuilder);

            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        public override string ServerAddress =>
            $"{this.dbConnectionStringBuilder["Host"]}:{this.dbConnectionStringBuilder["Port"]}";

        public override string DbEngine => "PostgreSQL";
    }
}
