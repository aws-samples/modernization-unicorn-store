# Secrets

To get started, you will need certain secrets for the Unicorn Store to function. Since our end goal is to deploy the Unicorn Store into a container that retrieves the connection string to RDS from AWS Secrets Manager and other sensitive information, let’s first understand the format of the secret. The good news is both AWS Secrets Manager and .NET Core Secret Manager store the secret in JSON. When the lab was provisioned, the CloudFormation template created and stored a secret in AWS Secrets Manager with a secret-id of **UNICORNSTORE_DBSECRET** that contains the credentials to the UnicornStoreRDS AWS::RDS::DBInstance. Below is an example of a secret for the credentials to our Unicorn Store RDS Database from AWS Secrets Manager. 

![examplesecret](/static/images/secrets/examplesecret.png)

For .NET Core secrets in the Secret Manager tool, the values are stored in a JSON configuration file in a system-protected user profile folder on the local machine:

| Linux/macOS File system path:                           | Windows File system path:                                      |
|---------------------------------------------------------|----------------------------------------------------------------|
| ~/.microsoft/usersecrets/<user_secrets_id>/secrets.json | %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json |

The <user_secrets_id> value is the value that is defined in the [UnicornStore.csproj](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/UnicornStore.csproj) file. This element has to be set in order to use user secrets in .NET Core. We have already set this value for you as defined below:

![csproj-usersecretsid](/static/images/secrets/csproj-usersecretsid.png)

Let's populate our local secrets.json development file for the .NET Core Secret Manager tool. We are going to query AWS Secrets Manager to retrieve the UNICORNSTORE_DBSECRET and populate our local development environment with the values so we can connect to our test database in RDS. Go ahead and run the below commands:

```
AWSSECRET=$(aws secretsmanager get-secret-value --secret-id UNICORNSTORE_DBSECRET | jq -r '.SecretString')

SECRET=$(printf '{"UNICORNSTORE_DBSECRET": %s}\n' $AWSSECRET)

mkdir -p ~/.microsoft/usersecrets/45b651b1-da6a-44fb-af93-525b292efddb/

echo $SECRET | jq . > ~/.microsoft/usersecrets/45b651b1-da6a-44fb-af93-525b292efddb/secrets.json
```

Check to make sure the file was created and the contents contain the secret. The format should be the same as above but have your unique secret values.

```
cat ~/.microsoft/usersecrets/45b651b1-da6a-44fb-af93-525b292efddb/secrets.json
```

As you can see, the secrets.json file is outside of your project meaning they will not be committed to any source control systems!

Click [**here**](/content/secrets/accessing-secrets.md) to move to the next section where we will explain how to access secrets in ASP.NET Core Secret Manager via the Configuration API.