<!--
+++
title = "Running App Locally"
menutitle = "Running Unicorn Store App Locally"
date = 2019-10-13T21:19:51-04:00
weight = 30
pre = "<b>2. </b>"
+++
-->
#### Starting Unicorn Store Web Application

|     | Notes |
| --- | ----- |
| ![App self-hosting](./images/app-self-host.png?width=900) | Once you have [prepared your development environment](./20-setting-up.html), what remains is to ensure that `UnicornStore.csproj` is selected as a startup project, to switch hosting from "IIS Express" to Console, as shown on the screenshot on the left, and *start the application* in the debug mode. |
| ![UnicornStor app open in browser](./images/unicorn-store-app-in-browser.png?width=900) | After debugging session has started, a command console window will open first with application console output in it, along with a web browser window, showing the home page of the Unicorn Store ASP.NET application. |

#### Exploring and Testing Unicorn Store Web Application Functionality

Unicorn Store is an emulation of an e-commerce web site implementing simplified product catalog, shopping cart and a checkout flow.

After you have started the application, you should see the home page shown on the screenshot above. At the bottom of the screen you should see RDBMS type and database server address information.

Please add a few unicorns to the basket, and click the "Checkout >>" button at the top of the shopping cart page. You will be prompted to log in. Please use credentials saved in the "secrets" file at the [previous step](20-setting-up.html): "`admin@acme.com`" for username, and "`Passw0rd!`" for the password. To complete checkout flow, please enter  shipping address, and for payment simply enter "**FREE**" in the "Promo Code" field and click "Submit Order" - if all went as planned, you should see the "Checkout Complete" message.
