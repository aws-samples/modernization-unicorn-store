using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UnicornStore.Configuration
{
    public class SqlDbContextOptionsConfigurator : DbContextOptionsConfigurator
    {
        public SqlDbContextOptionsConfigurator(
            DbConnectionStringBuilder dbConnectionStringBuilder,
            ILogger<SqlDbContextOptionsConfigurator> logger
            ) 
            : base(dbConnectionStringBuilder, logger)
        {
        }

        internal override void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            base.Configure(optionsBuilder);

            optionsBuilder.UseSqlServer(this.ConnectionString);
        }

        public override string ServerAddress
        {
            get
            {
                if (!this.dbConnectionStringBuilder.TryGetValue("Data Source", out object server))
                    server = this.dbConnectionStringBuilder["Server"];
                return server.ToString();
            }
        }

        public override string DbEngine => "MS SQL Server";
    }
}
