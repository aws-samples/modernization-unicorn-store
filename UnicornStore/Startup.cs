using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnicornStore.Components;
using UnicornStore.Models;
using System.Text.Json;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace UnicornStore
{
    public class Startup
    {
        private string _connection = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            // The UNICORNSTORE_DBSECRET is stored in AWS Secrets Manager
            // The value is loaded as an Environment Variable in a JSON string
            // The key/value pairs are mapped to the Configuration
            if (Configuration["UNICORNSTORE_DBSECRET"] != null)
            {
                var unicorn_envvariables = Configuration["UNICORNSTORE_DBSECRET"];
                var document = JsonDocument.Parse(unicorn_envvariables);
                var root = document.RootElement;
                Configuration["UNICORNSTORE_DBSECRET:username"] = root.GetProperty("username").GetString();
                Configuration["UNICORNSTORE_DBSECRET:password"] = root.GetProperty("password").GetString();
                Configuration["UNICORNSTORE_DBSECRET:host"] = root.GetProperty("host").GetString();
            }

            var sqlconnectionbuilder = new SqlConnectionStringBuilder(
                Configuration.GetConnectionString("UnicornStore"));
            sqlconnectionbuilder.Password = Configuration["UNICORNSTORE_DBSECRET:password"];
            sqlconnectionbuilder.UserID = Configuration["UNICORNSTORE_DBSECRET:username"];
            sqlconnectionbuilder.DataSource = Configuration["UNICORNSTORE_DBSECRET:host"];
            _connection = sqlconnectionbuilder.ConnectionString;

            services.AddDbContext<UnicornStoreContext>(options =>
                options.UseSqlServer(_connection));


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
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(_connection,
                    name: "UnicornDB-check",
                    tags: new string[] { "UnicornDB" });

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


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //This is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
            //The allowed values are Development,Staging and Production
            if (env.IsDevelopment())
            {
                // StatusCode pages to gracefully handle status codes 400-599.
                app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

                // Display custom error page in production when error occurs
                // During development use the ErrorPage middleware to display error information in the browser
                app.UseDeveloperExceptionPage();

                app.UseDatabaseErrorPage();
            }

            //This is invoked when ASPNETCORE_ENVIRONMENT is 'Production' or 'Staging'
            if (env.IsProduction() || env.IsStaging())
            {
                // StatusCode pages to gracefully handle status codes 400-599.
                app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHealthChecks("/health", new HealthCheckOptions()
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