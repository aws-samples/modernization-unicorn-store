using System;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using UnicornStore.Components;
using UnicornStore.Models;
using UnicornStore.HealthChecks;
using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.SqlClient;
using UnicornStore.Configuration;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.DataProtection;

namespace UnicornStore
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        internal IConfigurationSection ConnectionStringOverrideConfigSection => this.Configuration.GetSection("UnicornDbConnectionStringBuilder");

        public void ConfigureServices(IServiceCollection services)
        {
            this.ConfigureDatabaseEngine(services);
            services.AddDbContext<UnicornStoreContext>();

            services.Configure<AppSettings>(this.Configuration.GetSection("AppSettings"));

            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<UnicornStoreContext>()
                    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.AccessDeniedPath = "/Home/AccessDenied");

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://example.com");
                });
            });

            services.AddLogging();

            // Add MVC services to the services container
            services.AddMvc();

            services.AddOptions();

            // Add the Healthchecks
            services
                .AddHealthChecks()
                .AddCheck<UnicornHomePageHealthCheck>("UnicornStore_HealthCheck");

            // Add memory cache services
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddDataProtection()
                .PersistKeysToDbContext<UnicornStoreContext>(); // Not very efficient, consider using Redis for this.

            // Add session related services.
            services.AddSession();

            // Add the system clock service
            services.AddSingleton<ISystemClock, SystemClock>();

            // Configure Auth
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ManageStore",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("ManageStore", "Allowed");
                    });
            });
        }

        private void ConfigureDatabaseEngine(IServiceCollection services)
        {
#if MYSQL
            this.HookupMySQL(services);
#elif POSTGRES
            this.HookupPostgres(services);
#else
            this.HookupSqlServer(services);
#endif
        }

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
        private void HookupPostgres(IServiceCollection services)
        {
#if Debug || DEBUG
            // The line below is a compile-time debug feature for `docker build` outputting which database engine is hooked up 
#warning Using PostgreSQL for a database
#endif
            this.HookupDatabase<NpgsqlConnectionStringBuilder, NpgsqlDbContextOptionsConfigurator>(services, "Postgres");
        }
#else

        private void HookupSqlServer(IServiceCollection services)
        {
#if Debug || DEBUG
            // The line below is a compile-time debug feature for `docker build` outputting which database engine is hooked up 
#warning Using MS SQL Server for a database
#endif
            this.HookupDatabase<SqlConnectionStringBuilder, SqlDbContextOptionsConfigurator>(services, "SqlServer");
        }
#endif

        /// <summary>
        /// Integrates connection string data from config settings file "ConnectionStrings" section, 
        /// with override settings from the "UnicornDbConnectionStringBuilder" section of the config
        /// </summary>
        /// <remarks>
        /// This function ensures that connection string and connection string builder configuration
        /// is not cached as a singleton throughout the lifetime of the app, 
        /// but rather reloaded at run time whenever settings change.
        /// </remarks>
        /// <typeparam name="Tsb">A database engine specific DbConnectionStringBuilder subclass</typeparam>
        /// <typeparam name="Topt">A database engine specific DbContextOptionsConfigurator subclass</typeparam>
        /// <param name="services">DI container</param>
        /// <param name="databaseEngine">A suffix used for making a connection string name by appending it to the "UnicornStore" string</param>
        private void HookupDatabase<Tsb, Topt>(IServiceCollection services, string databaseEngine)
            where Tsb : DbConnectionStringBuilder, new()
            where Topt : DbContextOptionsConfigurator
        {
            Console.WriteLine($"Using {databaseEngine} for a database");
            string dbConnectionStringSettingName = $"UnicornStore{databaseEngine}";
            services.Configure<Tsb>(this.ConnectionStringOverrideConfigSection);
            services.AddScoped(di => DbConnectionStringBuilderFactory<Tsb>(di, dbConnectionStringSettingName));
            services.AddTransient<DbContextOptionsConfigurator, Topt>();
        }

        /// <summary>
        /// Combines default/common connection information supplied in the "ConnectionStrings.UnicornStore" configuration,
        /// with override values supplied by the "UnicornDbConnectionStringBuilder" configuration settings section.
        /// </summary>
        /// <param name="di">DI container</param>
        /// <returns></returns>
        internal DbConnectionStringBuilder DbConnectionStringBuilderFactory<T>(IServiceProvider di, string defaultConnectionStringName) 
            where T : DbConnectionStringBuilder, new()
        {
            DbConnectionStringBuilder overrideConnectionInfo = di.GetRequiredService<IOptionsSnapshot<T>>().Value;
            string defaultConnectionString = this.Configuration.GetConnectionString(defaultConnectionStringName);
            return overrideConnectionInfo.MergeDbConnectionStringBuilders(defaultConnectionString);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // StatusCode pages to gracefully handle status codes 400-599.
            app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

            //This is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
            //The allowed values are Development,Staging and Production
            if (env.IsDevelopment())
            {
                // Display custom error page in production when error occurs
                // During development use the ErrorPage middleware to display error information in the browser
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            //This is invoked when ASPNETCORE_ENVIRONMENT is 'Production' or 'Staging'
            if (env.IsProduction() || env.IsStaging())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHealthChecks("/health", ConfigureHealthCheckResponse());

            // force the en-US culture, so that the app behaves the same even on machines with different default culture
            var supportedCultures = new[] { new CultureInfo("en-US") };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.Use((context, next) =>
            {
                context.Response.Headers["Arch"] = RuntimeInformation.ProcessArchitecture.ToString();
                return next();
            });

            // Configure Session.
            app.UseSession();

            // Add static files to the request pipeline
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline
            app.UseAuthentication();

            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });
        }

        private static HealthCheckOptions ConfigureHealthCheckResponse()
        {
            return new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = JsonConvert.SerializeObject(
                        new
                        {
                            OverallStatus = report.Status.ToString(),
                            HealthChecks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                value = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                                status = e.Value.Description,
                                duration = e.Value.Duration
                            })
                        });
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            };
        }
    }
}