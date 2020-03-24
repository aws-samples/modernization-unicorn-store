# Create an AWS account

#### Your account must have the ability to create new IAM roles and scope other IAM permissions.

1. If you don't already have an AWS account with Administrator access: [create one now by clicking here](https://aws.amazon.com/getting-started/)

2. Once you have an AWS account, ensure you are following the remaining workshop steps as an IAM user with administrator access to the AWS account:
[Create a new IAM user to use for the workshop](https://console.aws.amazon.com/iam/home?#/users$new)

3. Enter the user details:
![Create User](/static/images/secrets/prerequisites/iam-1-create-user.png)

4. Attach the AdministratorAccess IAM Policy:
![Attach Policy](/static/images/secrets/prerequisites/iam-2-attach-policy.png)

5. Click to create the new user:
![Confirm User](/static/images/secrets/prerequisites/iam-3-create-user.png)

6. Take note of the login URL and save:
![Login URL](/static/images/secrets/prerequisites/iam-4-save-url.png)


You are now ready to launch the CloudFormation resources needed for this workshop. Click [**here**](cloudformation.md) to move to the next section where we will launch the CloudFormation resources. 
