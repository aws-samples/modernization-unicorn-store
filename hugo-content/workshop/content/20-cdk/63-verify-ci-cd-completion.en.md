<!--
+++
title = "Verify Pipeline Run Completion"
date = 2019-10-15T17:48:00-04:00
weight = 63
pre="<b>6. </b>"
+++
-->
While the pipeline is busy building the application and provisioning application hosting infrastructure, we have about 15-20 minutes to start hacking the app and CDK projects, adding MySQL support to it. Because this hosting infra build-out process takes some time, please feel free to go to move on to the next chapter and come back here later to ensure the pipeline is alright.

## Verifying the CI/CD Pipeline Run Has Completed 

Whenever [the pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) has finished, feel free to browse to the [AWS Load Balancer](https://console.aws.amazon.com/ec2/v2/home#LoadBalancers:sort=loadBalancerName) page in the AWS Console, select a load balancer named "unicorn-store" and click the "copy to clipboard" icon next to the "**DNS Name**" field.

Then paste the URL into a new browser tab, hit Enter and observe Unicorn Application home page loaded:
![Unicorn Store application in browser](./images/unicorn-store-app-in-browser.png) 

## Verifying the Unicorn Store App Running on AWS

To verify that nothing is broken, please add a few unicorns to the basket, and click the "Checkout >>" button at the top of the shopping cart page. You will be prompted to *log in*. Although you may be tempted to use your site admin credentials created [a few pages back](20-setting-up.html#application-configuration-for-postgresql), those credentials specified in the .NET User Secret were good only for *running locally*, and unlike settings stored in "appsettings.json", those credentials didn't travel through the build & deployment pipeline.

### Understanding Site Admin Username Setting Journey

Site admin username comes from "ProdEnvInfraAsCode" project "appsettings.json" file - take a look there and you will see site username set to "`Administrator@test.com`". "ProdEnvInfraAsCode" is a CDK project building our application hosting infrastructure, part of which is a an Amazon Elastic Container Service cluster, and that's how Container Definition for the app gets an environment variable "DefaultAdminUsername" set to "Administrator@test.com". *That's your site admin username*. You can see this value in AWS Console if you go to the ECS service -> "Unicorn-Store-ECS-Fargate-Cluster" service -> "InfraAsCodeStack..." Task Definition -> "Container Definitions" -> "web" -> "Environment Variables" -> "DefaultAdminUsername" key. Just a quick reminder: environment variables matching keys in appsettings.json, when passed to a .NET Core app, override "appsettings.json" values - that's how the application eventually gets expected site admin username setting value.

### Understanding Site Admin Password Setting Journey

Now, site administrator password - the "DefaultAdminPassword" app setting, was not specified in the "ProdEnvInfraAsCode" project "appsettings.json" file - feel free to take another look at the file to conirm. Instead, site admin password was auto-generated during the hosting infrastructure build-out and stored using Amazon [Secrets Manager](https://aws.amazon.com/secrets-manager/) service. 

You can see the AWS Secret referenced in AWS Console if you look at the Container Definition (see path above) and see the "DefaultAdminPassword" key. There instead of the value, you''l see AWS Secret Manager secret ARN, looking like "arn:aws:secretsmanager:us-east-1:123456789012:secret:UnicornSuperstoreDefaultSiteAdminPassword-mx7vMd"

To retrieve site admin password value from AWS Secrets Manager, please run this command:

```bash
aws secretsmanager get-secret-value --secret-id UnicornSuperstoreDefaultSiteAdminPassword | jq -r .SecretString
```
Copy the output of the command to the clipboard and use it as password to log in to the site. 

To complete checkout flow, please enter any shipping address, and for payment simply enter "FREE" in the "Promo Code" field and click "Submit Order" - if all went as planned, you should see the "Checkout Complete" message.

Later you may find it useful to practice on your own tracing these settings journeys through the code.