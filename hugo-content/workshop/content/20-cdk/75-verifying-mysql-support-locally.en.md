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
2. Build "UnicornStore" project - it should build without errors.
3. Now your local MySQL connection information may need to be added to the application settings in a non-destructive way, meaning without modifying "appSettings.json" or any other file in the solution root, which might lead to an inadvertent committing of credentials information to the version control system.<br/><br/>
*If you are taking this lab at an AWS event using AWS-supplied VM, you will not need to make any changes.* Please read through this paragraph  to understand what is accomplished here but make no changed, and move on the next bullet #5 below.<br/><br/>
To supply your local MySQL connection settings, we'll use .NET Secret Manager. Please right-click "UnicornStore" project and select `Manage User Secrets` from the context menu. That will open "secrets.json" file in the IDE editor.
  Create or update `UnicornDbConnectionStringBuilder` entry in the "secrets.json" as follows
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
4. Please be sure to replace the value of the `Password` field with your local MySQL password and save the file.
5. Run the "UnicornStore" project. If everything went well, you should see now-familiar Unicorn Store home page in the browser. 
  
  > At the very bottom of the page you should see "**MySQL database server address: localhost:3306**" message. If you've got it, congratulations! You've just made this run-off-the-mill ASP.NET Core application work with a mature, multi-platform, open-source database engine, [one implementation](https://aws.amazon.com/rds/aurora/mysql-features/) of which may give your database as much as 5x performance boost!