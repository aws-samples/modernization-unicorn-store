# Create Secrets

Now that you understand the basics of the .NET Core Secrets Manager tool, let's create two more secrets that are needed for the Unicorn Store. If you look at the [appsettings.Development.json](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/appsettings.Development.json) or [appsettings.Production.json](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/appsettings.Production.json) files, you'll notice two keys with the name ***DefaultAdminUsername*** and ***DefaultAdminPassword*** but neither have values. While you could set the values here directly, it would be insecure because then the files would be checked into source control with sensitive information inside. 

![secret-appsettings](/static/images/secrets/secret-appsettings.png)

When the lab was provisioned, the CloudFormation template created and stored both of those names in AWS Secrets Manager with a secret-id of ***DefaultAdminUsername*** and ***DefaultAdminUsername*** with a pre-set SecretString for each. We will use the values for SecretString locally in our testing via the .NET Core Secrets Manager tool. We will use AWS Secrets Manager later on in this lab when we deploy the Unicorn Store to AWS Fargate.

Let's set up the two secrets locally by issuing the following commands:

.NET Core Secrets Manager

```
cd ~/environment/modernization-unicorn-store/UnicornStore

AdminUser=$(aws secretsmanager get-secret-value --secret-id DefaultAdminUsername | jq -r '.SecretString')

dotnet user-secrets set "DefaultAdminUsername" $AdminUser

AdminPass=$(aws secretsmanager get-secret-value --secret-id DefaultAdminPassword | jq -r '.SecretString')

dotnet user-secrets set "DefaultAdminPassword" $AdminPass
```

Now that we have all of our secrets set up for local testing, let's look at the local secrets to ensure everything is ready. Your secrets should be the same as below but have your unique secret values:

```
dotnet user-secrets list
```

![secret-final](/static/images/secrets/secret-final.png)

Take note of all of the values by taking a screenshot or writing them down. You will need them later in this lab.

To test out the Unicorn Store in our local development environment using .NET Core Secrets Manager you can simply run the below command or run the command in an IDE like Visual Studio:

```
dotnet run
```

After a couple of seconds, you will see a message similar to the below.


However, in order to preview the application in Cloud9, click the ***Preview, Preview Running Application*** button on the menu bar in the AWS Cloud9 IDE.

![cloud9-dotnet](/static/images/secrets/cloud9-dotnet.png)

This opens an application preview tab within the environment, and then displays the application's output on the tab. Click the ***Pop Out Into New Window*** button on the preview tab so the page loads into a larger window.

![unicornstore](/static/images/unicornstore.png)

Try clicking the admin link at the bottom of the page and logging in with the secret values you set for ***DefaultAdminUsername*** and ***DefaultAdminPassword***. 

We've now successfully configured our development environment to authenticate to our Unicorn Store RDS instance. You may be wondering who created the database and how the application connected to it at this point. In the UnicornStore project, there is a class named [DBInitializer.cs](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/Data/DbInitializer.cs) in the Data folder that gets called via [Program.cs](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/Program.cs) when the application starts. If the database doesn't exist, the application creates it and populates it with sample data. The application connects to the database with the credentials you specified in .NET Secrets Manager because the application is running with an environment variable of ***"ASPNETCORE_ENVIRONMENT": "Development"*** which is set when you ran the application. 

Go ahead and stop the running .NET application by issuing a ***Ctrl+C*** in the Cloud9 Terminal.

Click [**here**](/content/secrets/containerize-unicornstore.md) to move to the next section where we will containerize the Unicorn Store and run it in Cloud9 using environment variables from Docker Compose.