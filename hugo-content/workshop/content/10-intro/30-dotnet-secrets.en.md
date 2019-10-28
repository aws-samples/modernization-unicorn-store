<!--
+++
title = ".NET Core Secret Manager"
menutitle = ".NET Core Secret Manager Note"
date = 2019-10-13T19:55:57-04:00
weight = 30
+++
-->

> An important note on using [.NET Core Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows#secret-manager):

This lab makes an extensive use of .NET Core Secret Manager on your development workstation to save database passwords and similar types of sensitive application configuration data that varies from one developer's workstation to another. Please note that despite the name **.NET Core Secret Manager is not secure!** It does not encrypt or password-protect your sensitive data. .NET Secrets are just plain text JSON files stored in a well-known directory. All they do is:

1. Allow saving applications settings data outside of the project root, eliminating the risk of committing your sensitive appsettings data inadvertently, and 
2. Provide one more layer in the appsettings.json override hierarchy.

One can think of *.NET Core Secret Manager as just a way to alter appsettings.json at development time, without modifying actual appsettings.json or launchsettings.json*, i.e. without the risk of committing sensitive setting values into a source code repository.
