<!--
+++
title = "MySQL in CI/CD CDK Project"
date = 2019-10-16T01:18:54-04:00
weight = 79
pre = "<b>7.7 </b>"
+++
-->
> Here we'll add MySQL support to the CI/CD CDK project.

1. Please start with marking "CicdInfraAsCode" project as a "Startup Project" in Visual Studio.
2. Open "`UnicornStoreCiCdStackProps.cs`" file in the IDE editor.
3. Find the `DbEngineType` enum and add `MySQL` entry to it.
4. Find the `DbEngine` property and replace the "`#if POSTGRES`" line with
```cs
#if MYSQL
            DbEngineType.MySQL;
#elif POSTGRES
```

This is it, that's all the changes we needed to make to the CI/CD CDK project to add MySQL database support.