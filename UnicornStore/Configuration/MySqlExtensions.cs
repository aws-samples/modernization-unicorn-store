using HealthChecks.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace UnicornStore.Configuration
{
    public static class MySqlExtensions
    {
        private const string defaultHealthCheckName = "mysql";

        public static IHealthChecksBuilder AddMySql(
          this IHealthChecksBuilder builder,
          Func<IServiceProvider, string> connectionStringFactory,
          string name = null,
          HealthStatus? failureStatus = null,
          IEnumerable<string> tags = null,
          TimeSpan? timeout = null)
        {
            return builder.Add(
                new HealthCheckRegistration(name ?? defaultHealthCheckName, 
                    sp => new MySqlHealthCheck(connectionStringFactory(sp)), 
                    failureStatus, 
                    tags, 
                    timeout
                )
            );
        }
    }
}
