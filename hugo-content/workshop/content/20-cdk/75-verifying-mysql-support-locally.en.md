<!--
+++
title = "Verifying MySQL Support"
menutitle = "Verifying MySQL Support Locally"
date = 2019-10-15T23:47:33-04:00
weight = 75
pre = "<b>7.5 </b>"
+++
-->
1. Please be sure that "DebugMySQL" solution configuration is active.
1. Build "UnicornStore" project - it should build without errors.
1. Now we need to add your local MySQL connection information to the application settings in a non-destructive way, meaning without modifying "appSettings.json" or any other file in the solution root, which might lead to an inadvertent committing of credentials information to the version control system. To do this, we'll use .NET Secret Manager. Please right-click "UnicornStore" project and select `Manage User Secrets` from the context menu. That will open "secrets.json" file in the IDE editor.
1. Create or update `UnicornDbConnectionStringBuilder` entry in the "secrets.json" as follows
```
{
        "UnicornDbConnectionStringBuilder": {
            "UserID": "root",
            "Password": "<PASTE YOUR LOCAL MYSQL PASSWORD HERE>",
            "ApplicationName": "Unicorn Store AWS"
        }
        ...
}
  ```
  5. Please be sure to replace the value of the `Password` field with your local MySQL password and save the file.
  5. Run the "UnicornStore" project. If everything went well, you should see now-familiar Unicorn Store home page in the browser. 
  
  > At the very bottom of the page you should see "**MySQL database server address: localhost:3306**" message. If you've got it, congratulations! You've just made this run-off-the-mill ASP.NET Core application work with a mature, multi-platform, open-source database engine, [one implementation](https://aws.amazon.com/rds/aurora/mysql-features/) of which may give your database as much as 5x performance boost!