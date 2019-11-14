using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnicornStore.Components;
using UnicornStore.Models;
using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.SqlClient;
using UnicornStore.Configuration;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UnicornStore
{
    public partial class Startup
    {
        const string dbHealthCheckName = "UnicornDB-check";
        static readonly string[] dbHealthCheckTags = { "UnicornDB" };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        internal IConfigurationSection ConnectionStringOverrideConfigSection => this.Configuration.GetSection("UnicornDbConnectionStringBuilder");

        public void ConfigureServices(IServiceCollection services)
        {
            IHealthChecksBuilder healthCheckBuilder = services.AddHealthChecks();
            
            this.ConfigureDatabaseEngine(services, healthCheckBuilder);
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
            services.AddControllersWithViews();
            services.AddRazorPages();
			
            services.AddOptions();

            // Add the Healthchecks
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
            // AspNetCore.Diagnostics.HealthChecks isn't maintained or supported by Microsoft.
            healthCheckBuilder
                .AddCheck("self", () => HealthCheckResult.Healthy())
                ;
					
            // Add memory cache services
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

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

        private void ConfigureDatabaseEngine(IServiceCollection services, IHealthChecksBuilder healthCheckBuilder)
        {
#if POSTGRES
            this.HookupPostgres(services, healthCheckBuilder);
#else
            this.HookupSqlServer(services, healthCheckBuilder);
#endif
        }

#if POSTGRES
        private void HookupPostgres(IServiceCollection services, IHealthChecksBuilder healthCheckBuilder)
        {
#if Debug || DEBUG
            // The line below is a compile-time debug feature for `docker build` outputting which database engine is hooked up 
#warning Using PostgreSQL for a database
#endif
            this.HookupDatabase<NpgsqlConnectionStringBuilder, NpgsqlDbContextOptionsConfigurator>(services, "Postgres");
            healthCheckBuilder.AddNpgSql(GetConnectionString, name: dbHealthCheckName, tags: dbHealthCheckTags);
        }
#else

        private void HookupSqlServer(IServiceCollection services, IHealthChecksBuilder healthCheckBuilder)
        {
#if Debug || DEBUG
            // The line below is a compile-time debug feature for `docker build` outputting which database engine is hooked up 
#warning Using MS SQL Server for a database
#endif
            this.HookupDatabase<SqlConnectionStringBuilder, SqlDbContextOptionsConfigurator>(services, "SqlServer");
            healthCheckBuilder.AddSqlServer(GetConnectionString, name: dbHealthCheckName, tags: dbHealthCheckTags);
        }
#endif

        internal static string GetConnectionString(IServiceProvider di)
        {
            var dbOptionConfig = di.GetRequiredService<DbContextOptionsConfigurator>();
            return dbOptionConfig.ConnectionString;
        }

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
			
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
			
            // Add the endpoint routing matcher middleware to the request pipeline
            app.UseRouting();			

            // Add cookie-based authentication to the request pipeline
            app.UseAuthentication();

            // Add the authorization middleware to the request pipeline
            app.UseAuthorization();

            // Add MVC to the request pipeline
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapAreaControllerRoute(
                    "admin",
                    "admin",
                    "Admin/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "api",
                    pattern: "{controller=Home}/{id?}");
            });
        }
    }
}