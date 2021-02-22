using Microsoft.Extensions.Configuration;
using System;

namespace CdkLib
{
    public static class Configuration
    {
        public const bool isDebug =
#if Debug || DEBUG
        true;
#else
        false;
#endif

        /// <summary>
        /// Integrates configuration settings from appsettings.json, command line args,
        /// environment variables, and .NET "secret" manager (in Development mode only).
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        public static IConfiguration InitConfiguration(this Type programClassType, string[] cmdLineArgs)
        {
            string envName = isDebug ? "Development" : "Production";

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{envName}.json", optional: true);
            builder.AddCommandLine(cmdLineArgs);

#pragma warning disable CS0162 // Unreachable code detected
            if (isDebug)
            {   // In Debug configuration, let User Secrets override Env Vars
                builder.AddEnvironmentVariables();
                builder.AddUserSecrets(programClassType.Assembly, optional: true);
            }else
            {   // In Release configuration, let Env Vars override User Secrets
                builder.AddUserSecrets(programClassType.Assembly, optional: true);
                builder.AddEnvironmentVariables();
            }
#pragma warning restore CS0162 // Unreachable code detected

            return builder.Build();
        }

        /// <summary>
        /// Loads configuration settings using standard .NET Core avenues:
        /// appsettings.json, command-line arguments and environment variables.
        /// </summary>
        /// <typeparam name="T">Class defining application settings</typeparam>
        /// <param name="programClassType">Class containing Main() method</param>
        /// <param name="cmdLineArgs">Application command line arguments</param>
        /// <returns></returns>
        public static T LoadConfiguration<T>(this Type programClassType, string[] cmdLineArgs)
            where T : BetterStackProps, new()
        {
            IConfiguration configuration = programClassType.InitConfiguration(cmdLineArgs);

            T settings = new T();
            settings.IsDebug = isDebug;
            configuration.Bind(settings);
            settings.PostLoadUpdateInternal();

            return settings;
        }
    }
}
