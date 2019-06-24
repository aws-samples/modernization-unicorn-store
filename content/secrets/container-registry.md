# Container Registry

Now that we've successfully containerized the Unicorn Store application, it's time to push the image to [Amazon Elastic Container Registry (ECR)](https://aws.amazon.com/ecr/) before we can deploy our application to an orchestrator like AWS Fargate. 

Amazon Elastic Container Registry (ECR) is a fully-managed Docker container registry that makes it easy for developers to store, manage, and deploy Docker container images. Amazon Elastic Container Registry integrates with Amazon ECS and the Docker CLI, allowing you to simplify your development and production workflows. You can easily push your container images to Amazon ECR using the Docker CLI from your development machine, and Amazon ECS can pull them directly for production deployments.

To get started, log into your Amazon ECR registry using the helper provided by the AWS CLI:

```
eval $(aws ecr get-login --no-include-email)
```

Use the AWS CLI to get information about the Amazon ECR repository for the Unicorn Store that was created for you ahead of time:

```
aws ecr describe-repositories --repository-name modernization-unicorn-store 
```

![unicornstore-ecr](/static/images/secrets/unicornstore-ecr.png)

Verify that the modernization-unicorn-store_unicornstore:latest image exists by running the following command:

```
docker images
```

![unicornstore-docker-image](/static/images/secrets/unicornstore-docker-images.png)


The next step is to tag your image so you can push the image to the repository by running the following command:

```
docker tag modernization-unicorn-store_unicornstore:latest $(aws ecr describe-repositories --repository-name modernization-unicorn-store --query=repositories[0].repositoryUri --output=text):latest
```

Run the following command to push your image to the repository:

```
docker push $(aws ecr describe-repositories --repository-name modernization-unicorn-store --query=repositories[0].repositoryUri --output=text):latest
```

With the Unicorn Store image now pushed to Amazon ECR, we are ready to deploy it to AWS Fargate.

Click [**here**](/content/secrets/taskdefinitions.md) to move to the next section where we will create the Task Definition for the Unicorn Store to run in AWS Fargate.
