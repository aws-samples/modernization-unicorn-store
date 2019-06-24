# Fargate

Now that we've successfully registered our Task Definition, it's time to deploy the Unicorn Store to [AWS Fargate](https://aws.amazon.com/fargate/).

To start, run the below commands to set some variables in order to successfully complete this chapter and run subsequent commands in the AWS CLI:

```
UnicornVPCID=$(aws ec2 describe-vpcs --filters Name=tag:Name,Values="Demo VPC" --query="Vpcs[0].VpcId" --output=text)

TaskSecurityGroup=$(aws ec2 describe-security-groups --filters Name=vpc-id,Values=$UnicornVPCID Name=group-name,Values=UnicornStoreTaskSecurityGroup --query "SecurityGroups[0].GroupId" --output=text) 

UnicornSubnet1=$(aws ec2 describe-subnets --filters Name=vpc-id,Values=$UnicornVPCID --query "Subnets[0].SubnetId" --output=text)

UnicornSubnet2=$(aws ec2 describe-subnets --filters Name=vpc-id,Values=$UnicornVPCID --query "Subnets[1].SubnetId" --output=text)

```

Now let's create a Target Group for the Application Load Balancer. When the lab was provisioned, the CloudFormation template created an Application Load Balancer with a name of ***UnicornStore-LB*** in your account. Notice the value of ***--target-type ip*** in the below command. This is because we are using the Fargate launch type and the task will use the awsvpc network mode. This is mandatory because tasks that use awsvpc network mode are associated with an elastic network interface, not an Amazon EC2 instance. 

```
aws elbv2 create-target-group --name ecs-Unicor-UnicornStore-Service --protocol HTTP --port 80 --vpc-id $UnicornVPCID --health-check-path "/health" --target-type ip
```

Let's create a listener for the Application Load Balancer. A listener is a process that checks for connection requests, using the protocol and port that you configure. The rules that you define for a listener determine how the load balancer routes requests to the targets in one or more target groups.

```
LBARN=$(aws elbv2 describe-load-balancers --names="UnicornStore-LB" --query="LoadBalancers[0].LoadBalancerArn" --output=text)

TGARN=$(aws elbv2 describe-target-groups --names ecs-Unicor-UnicornStore-Service --query="TargetGroups[0].TargetGroupArn" --output=text)

aws elbv2 create-listener --load-balancer-arn $LBARN --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=$TGARN

```

Now that the load balancer is ready, let's create the ECS Fargate Service for the Unicorn Store.

```
aws ecs create-service --cluster UnicornStoreCluster --service-name UnicornStore-Service --task-definition modernization-unicorn --desired-count 1 --launch-type "FARGATE" --network-configuration "awsvpcConfiguration={subnets=[$UnicornSubnet1, $UnicornSubnet2],securityGroups=[$TaskSecurityGroup],assignPublicIp=ENABLED}" --load-balancers targetGroupArn=$TGARN,containerName=modernization-unicorn-store_unicornstore,containerPort=80

```

Run the below commands to get the URL for the Unicorn Store behind the ALB and paste it into your browser. It may take up to a minute or so for the initial registration and the URL to the Unicorn Store to be healthy and ready to serve traffic:

```
LBDNS=$(printf "http://%s\n" $(aws elbv2 describe-load-balancers --names="UnicornStore-LB" --query="LoadBalancers[0].DNSName" --output=text))

until [[ `aws elbv2 describe-target-health --target-group-arn $TGARN --query "TargetHealthDescriptions[0].[TargetHealth]" --output text` == "healthy" ]]; do  echo "The Unicorn Store is NOT registered with the Target Group at `date`";   sleep 10; done && echo "The Unicorn Store is ready at `date` - Please proceed to $LBDNS"
```

***CONGRATULATIONS!!!*** You've now successfully deployed the ASP.NET Core Unicorn Store to AWS Fargate with the Production configuration coming from AWS Secrets Manager!

![unicornstore-prod](/static/images/secrets/unicornstore-prod.png)


