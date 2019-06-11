using System.Data.SqlClient;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnicornStore.Components;
using UnicornStore.Models;
using UnicornStore.HealthChecks;

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
                JObject parsed_json = JObject.Parse(unicorn_envvariables);
                Configuration["UNICORNSTORE_DBSECRET:username"] = (string)parsed_json["username"];
                Configuration["UNICORNSTORE_DBSECRET:password"] = (string)parsed_json["password"];
                Configuration["UNICORNSTORE_DBSECRET:host"] = (string)parsed_json["host"];
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
            services.AddMvc();

            services.AddOptions();

            // Add the Healthchecks
            services.AddHealthChecks()
                .AddCheck<UnicornHomePageHealthCheck>("UnicornStore_HealthCheck");

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


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            if (env.IsProduction() || env.IsStaging() )
            {
                // StatusCode pages to gracefully handle status codes 400-599.
                app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHealthChecks("/health",
                new HealthCheckOptions
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
    }
}