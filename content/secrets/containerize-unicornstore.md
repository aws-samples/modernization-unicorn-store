# Containerize the Unicorn Store

Now that we've successfully launched the Unicorn Store in our local development environment, it's time to containerize the application to get it ready to deploy to AWS Fargate. We will be using AWS Secrets Manager to inject the secrets into the container when it is initially started. The secrets will be injected as environment variables containing the sensitive data to present to the container.

If you remember from earlier in the lab, environment variables can be accessed via the the Configuration Provider in .NET Core. This means we can containerize the application without any sensitive information stored inside the container. When the container is launched, we can inject the senstive information in the format that the application is expecting. 

To build our container, we will use [Docker Compose](https://docs.docker.com/compose/) which is a tool for defining and running multi-container Docker applications. In the root of the modernization-unicorn-store repository you will see a file called [docker-compose.yml](https://github.com/aws-samples/modernization-unicorn-store/blob/master/docker-compose.yml). Feel free to review the contents. You will notice it uses [Dockerfile](https://github.com/aws-samples/modernization-unicorn-store/blob/master/UnicornStore/Dockerfile) in the UnicornStore project to build the application into a container. Issue the following command to build the container image:

```
cd ~/environment/modernization-unicorn-store/

docker-compose build
```

Once the image has been successfully built you should be able to see the modernization-unicorn-store_unicornstore:latest image by issuing the following command:

```
docker images
```

![unicornstore-docker-image](/static/images/secrets/unicornstore-docker-images.png)

At this point, you can launch the container locally with docker-compose. You'll notice that in the root of the modernization-unicorn-store repository there are two files named  [docker-compose.development.yml](https://github.com/aws-samples/modernization-unicorn-store/blob/master/docker-compose.development.yml) and [docker-compose.production.yml](https://github.com/aws-samples/modernization-unicorn-store/blob/master/docker-compose.production.yml). A common use case for multiple compose files is changing a development Compose app for a production-like environment. Keep in mind, you should ***NEVER*** store sensitive information in plain text in those files so they aren't accidentally committed to a source code repository. To showcase how multiple Compose files can be used together try uncommenting the lines in ***docker-compose.development.yml*** so they look like the below example and save the file.

You will need to replace the db specific information in the UNICORNSTORE_DBSECRET key with your specific information that you noted down. If you forgot to write them down, simply issue the following command:

```
dotnet user-secrets list -p ~/environment/modernization-unicorn-store/UnicornStore/

```

To edit the docker-compose-development.yml file with your values simply double click it on the left navigation tree in Cloud9 which opens up a text editor. Make sure you replace the following values in the UNICORNSTORE_DBSECRET json string in the docker-compose-development.yml. 

* username
* password
* host
* dbInstanceIdentifier

The file should look like the below once edited and saved with your values.

![docker-compose-development](/static/images/secrets/docker-compose-development.png)

Now run the following command:

```
cd ~/environment/modernization-unicorn-store
docker-compose -f docker-compose.yml -f docker-compose.development.yml up
```

You should be able to navigate to the Unicorn Store by clicking the ***Preview, Preview Running Application*** button on the menu bar in the AWS Cloud9 IDE. However, now the application is running in a local container. The necessary configuration has now been inserted as environment variables when the container was started just like when we go to launch it in Fargate.

Go ahead and stop the running .NET application by issuing a ***Ctrl+C*** in the Cloud9 Terminal.

Click [**here**](/content/secrets/container-registry.md) to move to the next section where we will push the Unicorn Store Docker image to Amazon Elastic Container Registry.
