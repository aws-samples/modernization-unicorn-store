using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfraAsCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App(null);
            new InfraAsCodeStack(app, "InfraAsCodeStack", new StackProps());
            app.Synth();
        }
    }
}
