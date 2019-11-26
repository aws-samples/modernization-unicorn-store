<!--
+++
title = "MySQL Support"
menutitle = "Adding MySQL Database Support"
date = 2019-10-15T15:42:30-04:00
pre = "<b>7. </b>"
weight = 65
+++
-->
This chapter will guide you through the process of adding support for [MySQL](https://www.mysql.com/) to the Unicorn Store application and to infra-as-code projects. `MySQL` is an open source, cross-platform, relational database that can be run in a cloud, on premises, or locally on developer workstation free of charge.

This set of steps builds on the existing, multi-database-friendly architecture of the Unicorn Store application, which already supports MS SQL Server and open-source PostgreSQL, while requiring only modest changes to the code of executable projects in the Solution.

> Although the amount of work to add C# code enabling MySQL support is fairly modest, if you'd rather not do that, feel free to fast-forward by following these steps:
>
> 1. Check out `cdk-module-completed` Git branch, that has all required changes, so you could run the app locally and/or compare `cdk-module` git branch with `cdk-module-completed`.
> 2. Use AWS online Console to modify "Source" stage of the pipeline, by editing the Action and changing the value of the "Branch Name" field from "cdk-module" to "`cdk-module-completed`".
> 3. Go to the [Testing CI/CD Project Changes](./80-verifying-cicd-project-changes.html) chapter to see how to deliver changes via the CI/CD pipeline.