
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnicornStore.Components;
using UnicornStore.Models;
using UnicornStore.HealthChecks;
using System.Data.SqlClient;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;


namespace UnicornStore
{
    public class Startup
    {
        private readonly Platform _platform;
        private string _connection = null;

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            // Below code demonstrates usage of multiple configuration sources. For instance a setting say 'setting1'
            // is found in both the registered sources, then the later source will win. By this way a Local config
            // can be overridden by a different setting while deployed remotely.
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                //.AddJsonFile("config.json")
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                //All environment variables in the process's context flow in as configuration values.
                .AddEnvironmentVariables();

            if (hostingEnvironment.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
            _platform = new Platform();
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Control whether InMemoryStore is used on Mac or for local development with AppSettings
            if (_platform.UseInMemoryStore)
            {
                // For local testing on Mac with User Secrets
                // Read about secrets here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-2.2
                if (Configuration.GetSection("AppSettings")["UseLocalSecrets"] == "True")
                {
                    var sqlconnectionbuilder = new SqlConnectionStringBuilder();
                    sqlconnectionbuilder.ConnectionString = "Database=UnicornStore;Trusted_Connection=False;MultipleActiveResultSets=true;Connect Timeout=30;";
                    sqlconnectionbuilder.Password = Configuration["unicornstore-secret:password"];
                    sqlconnectionbuilder.UserID = Configuration["unicornstore-secret:username"];
                    sqlconnectionbuilder.DataSource = Configuration["unicornstore-secret:host"];
                    _connection = sqlconnectionbuilder.ConnectionString;

                    services.AddDbContext<UnicornStoreContext>(options =>
                        options.UseSqlServer(_connection));
                }

                // Fallback to use InMemoryStore
                else if (Configuration.GetSection("AppSettings")["UseInMemoryStore"] == "True")
                {
                    services.AddDbContext<UnicornStoreContext>(options =>
                    options.UseInMemoryDatabase("Scratch"));
                }

            }
            // Control whether to LocalDB is used on Windows boxes for local devleopment with AppSettings
            else if (_platform.IsRunningOnWindows)
            {
                // For local testing on Windows with User Secrets
                // Read about secrets here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-2.2
                if (Configuration.GetSection("AppSettings")["UseLocalSecrets"] == "True")
                {
                    var sqlconnectionbuilder = new SqlConnectionStringBuilder();
                    sqlconnectionbuilder.ConnectionString = "Database=UnicornStore;Trusted_Connection=False;MultipleActiveResultSets=true;Connect Timeout=30;";
                    sqlconnectionbuilder.Password = Configuration["unicornstore-secret:password"];
                    sqlconnectionbuilder.UserID = Configuration["unicornstore-secret:username"];
                    sqlconnectionbuilder.DataSource = Configuration["unicornstore-secret:host"];
                    _connection = sqlconnectionbuilder.ConnectionString;

                    services.AddDbContext<UnicornStoreContext>(options =>
                        options.UseSqlServer(_connection));
                }

                // Fallback to use LocalDB. Connection string must be set in the appsettings.
                else if (Configuration.GetSection("AppSettings")["UseLocalDB"] == "True")
                {
                    services.AddDbContext<UnicornStoreContext>(options =>
                    options.UseSqlServer(Configuration[StoreConfig.ConnectionStringKey.Replace("__", ":")]));
                }
            }

            // Control whether to run the container in Fargate. 
            // Relies on RDS Secrets from AWS Secrets Manager to be passed as Environment Variables to the container
            if (Configuration.GetSection("AppSettings")["RunInFargate"] == "True")
            {
                var unicorn_envvariables = Configuration["unicornstore-secret"];
                JObject parsed_json = JObject.Parse(unicorn_envvariables);

                var sqlconnectionbuilder = new SqlConnectionStringBuilder();
                sqlconnectionbuilder.ConnectionString = "Database=UnicornStore;Trusted_Connection=False;MultipleActiveResultSets=true;Connect Timeout=30;";
                sqlconnectionbuilder.Password = (string)parsed_json["password"];
                sqlconnectionbuilder.UserID = (string)parsed_json["username"];
                sqlconnectionbuilder.DataSource = (string)parsed_json["host"];
                _connection = sqlconnectionbuilder.ConnectionString;

                services.AddDbContext<UnicornStoreContext>(options =>
                    options.UseSqlServer(_connection));
            }

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


            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = "550624398330273";
                    options.AppSecret = "10e56a291d6b618da61b1e0dae3a8954";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = "995291875932-0rt7417v5baevqrno24kv332b7d6d30a.apps.googleusercontent.com";
                    options.ClientSecret = "J_AT57H5KH_ItmMdu0r6PfXm";
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = "lDSPIu480ocnXYZ9DumGCDw37";
                    options.ConsumerSecret = "fpo0oWRNc3vsZKlZSq1PyOSoeXlJd7NnG4Rfc94xbFXsdcc3nH";
                })
            // The MicrosoftAccount service has restrictions that prevent the use of
            // http://localhost:5001/ for test applications.
            // As such, here is how to change this sample to uses http://ktesting.com:5001/ instead.

            // From an admin command console first enter:
            // notepad C:\Windows\System32\drivers\etc\hosts
            // and add this to the file, save, and exit (and reboot?):
            // 127.0.0.1 ktesting.com

            // Then you can choose to run the app as admin (see below) or add the following ACL as admin:
            // netsh http add urlacl url=http://ktesting:5001/ user=[domain\user]

            // The sample app can then be run via:
            // dnx . web
                .AddMicrosoftAccount(options =>
                {
                    // MicrosoftAccount requires project changes
                    options.ClientId = "000000004012C08A";
                    options.ClientSecret = "GaMQ2hCnqAC6EcDLnXsAeBVIJOLmeutL";
                });
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
        //The allowed values are Development,Staging and Production
        public void ConfigureDevelopment(IApplicationBuilder app)
        {
            // StatusCode pages to gracefully handle status codes 400-599.
            app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

            // Display custom error page in production when error occurs
            // During development use the ErrorPage middleware to display error information in the browser
            app.UseDeveloperExceptionPage();

            app.UseDatabaseErrorPage();

            Configure(app);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Staging'
        //The allowed values are Development,Staging and Production
        public void ConfigureStaging(IApplicationBuilder app)
        {
            // StatusCode pages to gracefully handle status codes 400-599.
            app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

            app.UseExceptionHandler("/Home/Error");

            Configure(app);
        }

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Production'
        //The allowed values are Development,Staging and Production
        public void ConfigureProduction(IApplicationBuilder app)
        {
            // StatusCode pages to gracefully handle status codes 400-599.
            app.UseStatusCodePagesWithRedirects("~/Home/StatusCodePage");

            app.UseExceptionHandler("/Home/Error");

            Configure(app);
        }

        public void Configure(IApplicationBuilder app)
        {
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

            //Populates the UnicornStore sample data
            SampleData.InitializeUnicornStoreDatabaseAsync(app.ApplicationServices).Wait();
        }
    }
}