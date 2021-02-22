using Amazon.CDK;
using System;
using System.Collections.Generic;

namespace CdkLib
{
    public class BetterStackProps : StackProps
    {
        protected BetterStackProps(string defaultScopeName)
        {
            this.ScopeName = defaultScopeName;

            if (this.Tags == null)
                this.Tags = new Dictionary<string, string>();
        }

        /// <summary>
        /// This is the string containing prefix for many different 
        /// CDK Construct names/IDs. Default value is "UnicornStore".
        /// </summary>
        public string ScopeName { get; set; }

        public bool IsDebug { get; set; } =
#if Debug || DEBUG
            true;
#else
            false;
#endif

        internal void PostLoadUpdateInternal()
        {
            if (string.IsNullOrWhiteSpace(this.ScopeName))
                throw new ArgumentException($"Scope name cannot be blank");

            if (string.IsNullOrWhiteSpace(this.StackName))
                this.StackName = $"{this.ScopeName}Stack";

            if (!this.Tags.TryGetValue("Scope", out string scope) || string.IsNullOrWhiteSpace(scope))
                this.Tags["Scope"] = this.ScopeName;

            this.PostLoadUpdate();
        }

        protected virtual void PostLoadUpdate()
        {
        }
    }
}
