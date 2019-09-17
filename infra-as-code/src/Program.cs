using Microsoft.Extensions.Configuration;
using Amazon.CDK;
using Amazon.CDK.CXAPI;
using System;

namespace InfraAsCode
{
    class Program
    {
        static void Main(string[] args)
        {
            UnicornStoreFargateStackProps settings = LoadConfiguration(args);

            var app = new App(null);
            new UnicornStoreFargateStack(app, "InfraAsCodeStack", settings);
            CloudAssembly infra = app.Synth();
            Console.WriteLine($"Synthesized to \"{infra.Directory}\".");
        }

        /// <summary>
        /// Loads configuration settings using standard .NET Core avenues:
        /// appsettings.json, command-line arguments and environment variables.
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns><see cref="IConfiguration"/> deserialized into <see cref="UnicornStoreFargateStackProps"/></returns>
        private static UnicornStoreFargateStackProps LoadConfiguration(string[] cmdLineArgs)
        {
            IConfiguration configuration = InitConfiguration(cmdLineArgs);

            UnicornStoreFargateStackProps settings = new UnicornStoreFargateStackProps();
            configuration.Bind(settings);

            InitSettingDefaults(settings);

            return settings;
        }

        private static void InitSettingDefaults(UnicornStoreFargateStackProps settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ScopeName))
                settings.ScopeName = UnicornStoreFargateStackProps.DefaultScopeName;

            if (string.IsNullOrWhiteSpace(settings.StackName))
                settings.StackName = $"{settings.ScopeName}Stack";

            if (!settings.Tags.TryGetValue("Scope", out string scope) || string.IsNullOrWhiteSpace(scope))
                settings.Tags["Scope"] = settings.ScopeName;

#if MYSQL
            settings.DbEngine = UnicornStoreFargateStackProps.DbEngineType.MYSQL;
#elif POSTGRES
            settings.DbEngine = UnicornStoreFargateStackProps.DbEngineType.POSTGRES;
#else
            settings.DbEngine = UnicornStoreFargateStackProps.DbEngineType.SQLSERVER;
#endif
        }

        /// <summary>
        /// Integrates configuration settings from appsettings.json, command line args,
        /// environment variables, and .NET "secret" manager (in Development mode only).
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        private static IConfiguration InitConfiguration(string[] cmdLineArgs)
        {
            bool isDevEnv; 
#if Debug || DEBUG
            isDevEnv = true;
#else
            isDevEnv = false;
#endif
            string envName = isDevEnv ? "Development" : "Production";

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{envName}.json", optional: true);
            builder.AddCommandLine(cmdLineArgs);
            builder.AddEnvironmentVariables();
            if (isDevEnv)
            {
                // To enable "Manage User Secrets" project menu item, add dependency on the 
                // "Microsoft.Extensions.Configuration.UserSecrets" NuGet package first.
                builder.AddUserSecrets(typeof(Program).Assembly, optional: true);
            }
            return builder.Build();
        }
    }
}
