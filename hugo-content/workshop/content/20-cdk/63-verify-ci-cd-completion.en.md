<!--
+++
title = "Verify Pipeline Run Completion"
date = 2019-10-15T17:48:00-04:00
weight = 63
pre="<b>6. </b>"
+++
-->
While the pipeline is busy building the application and provisioning application hosting infrastructure, we have about 15-20 minutes to start hacking the app and CDK project, adding MySQL support to it. Feel free to go to the next chapter and come back here later to ensure the pipeline is alright.

#### Verifying the CI/CD Pipeline Run Has Completed 

Whenever [the pipeline](https://console.aws.amazon.com/codesuite/codepipeline/home) has finished, feel free to browse to the [AWS Load Balancer](https://console.aws.amazon.com/ec2/v2/home#LoadBalancers:sort=loadBalancerName) page in the AWS Console, select a load balancer named "unicorn-store" and click the "copy to clipboard" icon next to the "**DNS Name**" field.

Then paste the URL into a new browser tab, hit Enter and observe Unicorn Application home page loaded:
![Unicorn Store application in browser](./images/unicorn-store-app-in-browser.png) 

#### Verifying the Unicorn Store App Running on AWS

To verify that nothing is broken, please add a few unicorns to the basket, and click the "Checkout >>" button at the top of the shopping cart page. You will be prompted to *log in*. Although site administrator username username has not changed and remains "Administrator@test.com", password is was not specified explicitly in the hosting environment infrastructure project configuration or anywhere else for that matter. Instead, password was stored using Amazon [Secrets Manager](https://aws.amazon.com/secrets-manager/) service. 

To retrieve site admin password from AWS Secrets Manager, please run this command:

```bash
aws secretsmanager get-secret-value --secret-id UnicornSuperstoreDefaultSiteAdminPassword | jq -r .SecretString
```
Copy the output of the command to the clipboard and use it as password to log in to the site. 

To complete checkout flow, please enter shipping address, and for payment simply enter "FREE" in the "Promo Code" field and click "Submit Order" - if all went as planned, you should see the "Checkout Complete" message.