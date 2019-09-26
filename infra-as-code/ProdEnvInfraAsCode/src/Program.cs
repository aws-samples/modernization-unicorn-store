using Amazon.CDK;
using Amazon.CDK.CXAPI;
using System;
using CdkLib;

namespace ProdEnvInfraAsCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = typeof(Program).LoadConfiguration<UnicornStoreDeploymentEnvStackProps>(args, InitSettingDefaults);

            var app = new App(null);
            new UnicornStoreFargateStack(app, "InfraAsCodeStack", settings);
            CloudAssembly infra = app.Synth();
            Console.WriteLine($"Synthesized to \"{infra.Directory}\".");
        }

        private static void InitSettingDefaults(UnicornStoreDeploymentEnvStackProps settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ScopeName))
                settings.ScopeName = UnicornStoreDeploymentEnvStackProps.DefaultScopeName;

            if (string.IsNullOrWhiteSpace(settings.StackName))
                settings.StackName = $"{settings.ScopeName}Stack";

            if (!settings.Tags.TryGetValue("Scope", out string scope) || string.IsNullOrWhiteSpace(scope))
                settings.Tags["Scope"] = settings.ScopeName;

#if MYSQL
            settings.DbEngine = UnicornStoreDeploymentEnvStackProps.DbEngineType.MySQL;
#elif POSTGRES
            settings.DbEngine = UnicornStoreDeploymentEnvStackProps.DbEngineType.Postgres;
#else
            settings.DbEngine = UnicornStoreDeploymentEnvStackProps.DbEngineType.SqlServer;
#endif
        }
    }
}
