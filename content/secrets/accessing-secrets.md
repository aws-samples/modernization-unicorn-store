# Accessing Secrets

Now that you've set up the secret for local development, you may be wondering how can you access a secret in your .NET Core code. The [ASP.NET Core Configuration API](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/index?view=aspnetcore-3.0) provides access to Secret Manager secrets.

In ASP.NET 2.0 or later, the user secrets configuration source is automatically added in development mode when the project calls [CreateDefaultBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.webhost.createdefaultbuilder) to initialize a new instance of the host with preconfigured defaults. Below is a snippet from our [Program.cs](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/Program.cs) file.

![program-createdefaultbuilder](/static/images/secrets/program-createdefaultbuilder.png)

Anytime we want to retrieve a user secret during local development, we can do so via the Configuration API. Below is a snippet from our [Startup.cs](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/Startup.cs) file. You'll notice that IConfiguration is injected into the Startup constructor to access configuration values. Once that's done, accessing a key/value for something like the password value is as simple as calling ***Configuration["UNICORNSTORE_DBSECRET:password"]***.

![startup-configuration](/static/images/secrets/startup-configuration.png)

You may have noticed in the above snippet of code that we are constructing our database connection string using [SqlConnectionStringBuilder](https://docs.microsoft.com/dotnet/api/system.data.sqlclient.sqlconnectionstringbuilder). This is best practice because now we aren't storing sensitive information like a password in plain text which is insecure. Look at the [appsettings.Development.json](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/appsettings.Development.json) file in our project. You'll notice that none of the sensitive information is in the connection string. Only a portion of what's needed is set there and the rest of the connection string can be driven by configuration based on environment.

![connectionstring](/static/images/secrets/connectionstring.png)

The Configuration API is a very powerful feature of .NET Core and can handle multiple configuration sources. When an ASP.NET Core application starts, it loads your configuration providers in the order they are configured. If a configuration source is loaded and the key already exists, it overwrites the previous value meaning the last key loaded wins.

In ***Program.cs*** there is a method called ***CreateDefaultBuilder*** which is behind the configuration provider setup. Looking at the [CreateDefaultBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder?view=dotnet-plat-ext-3.0) method, we can see that the order of providers are configured as follows:

1. Files (appsettings.json, appsettings.{Environment}.json, where {Environment} is the app's current hosting environment)
2. User secrets (Secret Manager) (in the Development environment only)
3. Environment variables
4. Command-line arguments

It's a common practice to position the Command-line Configuration Provider last in a series of providers to allow command-line arguments to override configuration set by the other providers.

This sequence of providers is put into place when you initialize a new WebHostBuilder with CreateDefaultBuilder.

Click [**here**](/content/secrets/create-secrets.md) to move to the next section where we will create some more local secrets for the Unicorn Store and run the application in your development environment.