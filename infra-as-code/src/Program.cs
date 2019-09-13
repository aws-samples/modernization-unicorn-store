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
        /// <param name="args"></param>
        /// <returns></returns>
        private static UnicornStoreFargateStackProps LoadConfiguration(string[] args)
        {
            IConfiguration configuration = InitConfiguration(args);
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
        }

        private static IConfiguration InitConfiguration(string[] args)
        {
            string envName;
#if Debug || DEBUG
            envName = "Development";
#else
            envName = "Production";
#endif

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{envName}.json", optional: true);
            builder.AddCommandLine(args);
            builder.AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
