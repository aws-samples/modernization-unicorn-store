<!--
+++
title = "App: NuGet and AppSettings"
date = 2019-10-15T16:13:30-04:00
weight = 10
pre = "<b>6.1. </b>"
+++
-->
### Adding MySQL Library to the Application via NuGet

1. Please start with marking "UnicornStore" project as a *Startup Project* in Visual Studio.
1. Open "Package Manager Console" by selecting it from the "View | Other Windows" menu.
1. Install [Pomelo.EntityFrameworkCore.MySql **version 2.2**](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/2.2.0) package by running following command in Visual Studio "Package Manager Console":
```bash
Install-Package Pomelo.EntityFrameworkCore.MySql -Version 2.2.0
```