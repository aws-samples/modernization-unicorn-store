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
            var settings = typeof(Program).LoadConfiguration<UnicornStoreProdEnvStackProps>(args, InitSettingDefaults);

            var app = new App(null);
            new UnicornStoreFargateStack(app, "InfraAsCodeStack", settings);
            CloudAssembly infra = app.Synth();
            Console.WriteLine($"Synthesized to \"{infra.Directory}\".");
        }

        private static void InitSettingDefaults(UnicornStoreProdEnvStackProps settings)
        {
            if (string.IsNullOrWhiteSpace(settings.ScopeName))
                settings.ScopeName = UnicornStoreProdEnvStackProps.DefaultScopeName;

            if (string.IsNullOrWhiteSpace(settings.StackName))
                settings.StackName = $"{settings.ScopeName}Stack";

            if (!settings.Tags.TryGetValue("Scope", out string scope) || string.IsNullOrWhiteSpace(scope))
                settings.Tags["Scope"] = settings.ScopeName;

#if MYSQL
            settings.DbEngine = UnicornStoreProdEnvStackProps.DbEngineType.MYSQL;
#elif POSTGRES
            settings.DbEngine = UnicornStoreFargateStackProps.DbEngineType.POSTGRES;
#else
            settings.DbEngine = UnicornStoreFargateStackProps.DbEngineType.SQLSERVER;
#endif
        }
    }
}
