# Launch the CloudFormation resources

To get started, you will need a S3 bucket to host the CloudFormation nested stack template for the VPC. To do this you can either create the bucket through the console and upload the template yourself or follow the below instructions using the AWS CLI.

1. Create the bucket. This workshop runs out of the us-west-2 so the `--region` flag is important so don't change it. Also, make sure you give a unique name for the `--bucket` value and write it down for later use.

```
aws s3api create-bucket --bucket modernization-unicorn-store --region us-west-2 --create-bucket-configuration LocationConstraint=us-west-2
```

2. If you haven't already done so, download the two CloudFormation templates in the `/content/secrets/cfn` directory in this repository to your local machine or just clone the repository.

3. Change to the `/content/secrets/cfn` directory and upload the `player-vpc-template` to the s3 bucket you just created. Only change the `--bucket` value to your bucket. Keep the rest of the command as is.

```
aws s3api put-object --bucket modernization-unicorn-store --key modules/modernization/unicorn-store/templates/player-vpc-template.yaml --body player-vpc-template.yaml
```

4. Deploy the CloudFormation template by running the below comannd. Only change the value for the `ParameterValue` key to your bucket. 

```
aws cloudformation create-stack \
--stack-name modernization-unicorn-store \
--parameters ParameterKey=BucketName,ParameterValue=modernization-unicorn-store ParameterKey=BucketPrefix,ParameterValue=modules/modernization/unicorn-store \
--template-body file://player-template.yaml \
--capabilities CAPABILITY_NAMED_IAM
```

5. The CloudFormation stack takes a while to run due to it provisioning a RDS instance. Run this command and wait for the CloudFormation template to finish deploying.

```
until [[ `aws cloudformation describe-stacks --stack-name "modernization-unicorn-store" --query "Stacks[0].[StackStatus]" --output text` == "CREATE_COMPLETE" ]]; do  echo "The stack is NOT in a state of CREATE_COMPLETE at `date`";   sleep 30; done && echo "The Stack is built at `date` - Please proceed"
```

Once you have completed with either setup, continue with [**Create a Workspace**](/content/secrets/prerequisites/getting-started.md)