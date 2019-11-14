using Amazon.CDK;
using Amazon.CDK.CXAPI;
using CdkLib;
using System;

namespace CicdInfraAsCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = typeof(Program).LoadConfiguration<UnicornStoreCiCdStackProps>(args);

            var app = new App(null);
            new CicdInfraAsCodeStack(app, "CicdInfraAsCodeStack", settings);
            CloudAssembly infra = app.Synth();
            Console.WriteLine($"Synthesized to \"{infra.Directory}\".");
        }
    }
}
