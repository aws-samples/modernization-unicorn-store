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
            var settings = typeof(Program).LoadConfiguration<UnicornStoreDeploymentEnvStackProps>(args);

            var app = new App(null);
            new UnicornStoreFargateStack(app, "InfraAsCodeStack", settings);
            CloudAssembly infra = app.Synth();
            Console.WriteLine($"Synthesized to \"{infra.Directory}\".");
        }
    }
}
