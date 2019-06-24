# Task Definitions

## Introduction

[Amazon ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html) are required to run Docker containers in Amazon ECS.

## Create the Unicorn Store Task Definition

Copy/Paste the following commands into your terminal.

```

cd ~/environment/modernization-unicorn-store/content/secrets/taskdefinitions.files/

```

Check the configuration of modernization-unicorn-store-task-definition.json by issuing the following command:

```
cat modernization-unicorn-store-task-definition.json | jq
```

You can see a couple of interesting key/values in this json file that defines our application. One of the most important sections is the ***secrets*** section. You will see here this is how are accessing AWS Secrets Manager. To inject sensitive data into your containers as environment variables, use the secrets container definition parameter.

![modernization-unicorn-store-task-definition](/static/images/secrets/modernization-unicorn-store-task-definition.png)

We need to replace the placeholders for your account id and the individual arn's of each secret in the task definition template file so that it works in your account. The below commands do it automatically for you:

Insert the AccountID for the image and the executionRoleArn:

```
ACCOUNT_ID=$(aws ecr describe-repositories --repository-name modernization-unicorn-store | jq -r '.repositories[] | {registryId}' | jq --raw-output '.registryId')

echo $ACCOUNT_ID

sed -i "s/<YourAccountID>/${ACCOUNT_ID}/" modernization-unicorn-store-task-definition.json
```

Insert the UNICORNSTORE_DBSECRET Secrets Manager Arn:

```
DBSECRET_ARN=$(aws secretsmanager describe-secret --secret-id UNICORNSTORE_DBSECRET | jq -r '.ARN')

echo $DBSECRET_ARN

sed -i "s/<arn:aws:secretsmanager:region:aws_account_id:secret:UNICORNSTORE_DBSECRET-AbCdEf>/${DBSECRET_ARN}/" modernization-unicorn-store-task-definition.json
```

Insert the DefaultAdminUsername Secrets Manager Arn:

```
AdminSecret_ARN=$(aws secretsmanager describe-secret --secret-id DefaultAdminUsername | jq -r '.ARN')

echo $AdminSecret_ARN

sed -i "s/<arn:aws:secretsmanager:region:aws_account_id:secret:DefaultAdminUsername-AbCdEf>/${AdminSecret_ARN}/" modernization-unicorn-store-task-definition.json
```

Insert the DefaultAdminPassword Secrets Manager Arn:

```
PasswordSecret_ARN=$(aws secretsmanager describe-secret --secret-id DefaultAdminPassword | jq -r '.ARN')

echo $PasswordSecret_ARN

sed -i "s/<arn:aws:secretsmanager:region:aws_account_id:secret:DefaultAdminPassword-AbCdEf>/${PasswordSecret_ARN}/" modernization-unicorn-store-task-definition.json
```

Go ahead and inspect the modernization-unicorn-store-task-definition.json file again to see that the configuration has been updated by issuing the following command:

```
cat modernization-unicorn-store-task-definition.json | jq
```

Now we can register the task definition with ECS from the JSON file by running the following command:

```
aws ecs register-task-definition --cli-input-json file://modernization-unicorn-store-task-definition.json
```

## Required IAM Permissions for Amazon ECS Secrets

You may be wondering how the task definition is going to access the secret at this point. It's as simple as adding a policy to the UnicornStoreExecutionRole IAM role that is specified in the task definition. When the lab was provisioned, the CloudFormation template created the IAM role for you with the appropriate permissions to provide access to the Secrets Manager resources.

Feel free to explore the IAM Role for UnicornStoreExecutionRole in the AWS console or via the cli and look at the ***RetrieveUnicornSecret*** Inline policy. It should have something similar to below.

![retrieveunicornsecret-policy](/static/images/secrets/retrieveunicornsecret-policy.png)

Click [**here**](/content/secrets/fargate.md) to move to the next section where we will create the service in AWS Fargate to run the Unicorn Store.
