using Amazon.CDK;

namespace CicdInfraAsCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);
            new CicdInfraAsCodeStack(app, "CicdInfraAsCodeStack", new StackProps());
            app.Synth();
        }
    }
}
