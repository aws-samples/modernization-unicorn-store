using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UnicornStore.Data;
using UnicornStore.Models;
using System.Threading.Tasks;

namespace UnicornStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<UnicornStoreContext>();
                    var env = services.GetRequiredService<IConfiguration>();
                    //Populates the UnicornStore sample data and creates the DB if it doesn't exist.
                    await DbInitializer.Initialize(context, services, env);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            await host.RunAsync();
        }

       public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddCommandLine(args);
                })
                .UseStartup<Startup>();
    }

}