<!--
+++
title = "App: NuGet and AppSettings"
date = 2019-10-15T16:13:30-04:00
weight = 67
pre = "<b>7.1 </b>"
+++
-->

> This chapter assumes that MySQL is installed locally.

#### Adding MySQL Library to the Application via NuGet

1. Please start with marking "UnicornStore" project as a *Startup Project* in Visual Studio.
1. Open "Package Manager Console" by selecting it from the "View | Other Windows" menu.
1. Install [Pomelo.EntityFrameworkCore.MySql **version 2.2**](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/2.2.0) package by running following command in Visual Studio "Package Manager Console":
```bash
Install-Package Pomelo.EntityFrameworkCore.MySql -Version 2.2.0
```

#### Adding Default Connection String to "appSettings.json"

Open "appSettings.json" file of the "UnicornStore" project and add following entry to the "`ConnectionStrings`" section.
```json
"UnicornStoreMySQL": "Server=localhost;Database=UnicornStore;Uid=root;Pwd=NeverEVERsavePasswordInConfigFiles;"
```
Don't forget the comma before or after the "UnicornStoreMySQL" property.