using Amazon.CDK.AWS.CodeBuild;
using System.Collections.Generic;
using System.Linq;

namespace CdkLib
{
    public class BuildSpecHelper
    {
        private const string defaultBuildSpecVersion = "0.2";

        public string Version { get; set; } = defaultBuildSpecVersion;

        public bool? GitCreadentialHelper { get; set; }

        public Dictionary<string, string> EnvVars { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public Dictionary<string, string> InstallRuntimes { get; set; }

        public string[] InstallCommands { get; set; }

        public string[] PreBuildCommands { get; set; }

        public string[] BuildCommands { get; set; }

        public string[] PostBuildCommands { get; set; }

        public Dictionary<string, object> Artifacts { get; set; }

        public string[] CachePaths { get; set; }

        public BuildSpec ToBuildSpec()
        {
            return BuildSpec.FromObject(this.ToBuildSpecObject());
        }

        private Dictionary<string, object> ToBuildSpecObject()
        {
            var buildSpec = new Dictionary<string, object>();

            buildSpec["version"] = this.Version ?? defaultBuildSpecVersion;

            AddSection(buildSpec, this.EnvVars, "env", "variables");
            AddSection(buildSpec, this.Parameters, "env", "parameter-store");

            if (this.GitCreadentialHelper.HasValue)
                AddSection(buildSpec, this.GitCreadentialHelper.Value ? "yes" : "no", 
                    "env", "git-credential-helper"
                );

            AddSection(buildSpec, this.InstallRuntimes, "phases", "install", "runtime-versions");
            AddSection(buildSpec, this.InstallCommands, "phases", "install", "commands");
            AddSection(buildSpec, this.PreBuildCommands, "phases", "pre_build", "commands");
            AddSection(buildSpec, this.BuildCommands, "phases", "build", "commands");
            AddSection(buildSpec, this.PostBuildCommands, "phases", "post_build", "commands");
            AddSection(buildSpec, this.CachePaths, "cache", "paths");

            string buildSpecJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                buildSpec, Newtonsoft.Json.Formatting.Indented
            );
            System.Diagnostics.Trace.TraceInformation($"Buildspec JSON:\n{buildSpecJson}");

            return buildSpec;
        }

        private static void AddSection<T>(IDictionary<string, object> buildSpec,
                                IDictionary<string, T> items,
                                params string[] sections)
        {
            if (items == null || items.Count == 0)
                return;

            foreach (var item in items)
                AddSection(buildSpec, 
                    item.Value,
                    sections.Concat(new[] { item.Key }).ToArray()
                );
        }

        private static void AddSection(IDictionary<string, object> buildSpec,
                                ICollection<string> items,
                                params string[] sections)
        {
            if (items == null || items.Count == 0)
                return;

            AddSection(buildSpec, (object)items, sections);
        }

        private static void AddSection(IDictionary<string, object> buildSpec,
                                object item,
                                params string[] sections
            )
        {
            if (item == null)
                return;

            int i = 0;
            object subsection;
            IDictionary<string, object> section;
            
            for (section = buildSpec; 
                i < sections.Length ; 
                i++, section = (IDictionary<string, object>)subsection
            )
            {
                string key = sections[i];
                if (!section.TryGetValue(key, out subsection))
                {
                    if (i == sections.Length - 1)
                        section[key] = item;
                    else
                    {
                        subsection = new Dictionary<string, object>();
                        section[key] = subsection;
                    }
                }
            }
        }
    }
}
