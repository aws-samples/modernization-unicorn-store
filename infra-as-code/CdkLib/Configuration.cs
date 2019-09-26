using Microsoft.Extensions.Configuration;
using System;

namespace CdkLib
{
    public static class Configuration
    {

        /// <summary>
        /// Integrates configuration settings from appsettings.json, command line args,
        /// environment variables, and .NET "secret" manager (in Development mode only).
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        public static IConfiguration InitConfiguration(this Type programClassType, string[] cmdLineArgs)
        {
            bool isDevEnv;
#if Debug || DEBUG
            isDevEnv = true;
#else
            isDevEnv = false;
#endif
            string envName = isDevEnv ? "Development" : "Production";

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{envName}.json", optional: true);
            builder.AddCommandLine(cmdLineArgs);
            builder.AddEnvironmentVariables();
            if (isDevEnv)
            {
                // To enable "Manage User Secrets" project menu item, add dependency on the 
                // "Microsoft.Extensions.Configuration.UserSecrets" NuGet package first.
                builder.AddUserSecrets(programClassType.Assembly, optional: true);
            }
            return builder.Build();
        }

        /// <summary>
        /// Loads configuration settings using standard .NET Core avenues:
        /// appsettings.json, command-line arguments and environment variables.
        /// </summary>
        /// <typeparam name="T">Class defining application settings</typeparam>
        /// <param name="programClassType">Class containing Main() method</param>
        /// <param name="cmdLineArgs">Application command line arguments</param>
        /// <param name="configInitializer">Optional configuration post-load initializer</param>
        /// <returns></returns>
        public static T LoadConfiguration<T>(this Type programClassType, string[] cmdLineArgs, Action<T> configInitializer = null)
            where T : new()
        {
            IConfiguration configuration = programClassType.InitConfiguration(cmdLineArgs);

            T settings = new T();
            configuration.Bind(settings);

            configInitializer?.Invoke(settings);

            return settings;
        }
    }
}
