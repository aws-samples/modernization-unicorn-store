using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

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
