Introduction
--------------------

This is a sample web application built on [ASP.NET Core MVC](https://github.com/dotnet/aspnetcore) using [ASPSecurityKit](https://ASPSecurityKit.net/) (a cross-platform IAM framework to rapidly build reliable web apps and API platforms with zero-trust based enterprise-grade security).

A customer relationship management (CRM) software is primarily used to manage contacts (customers) and the interactions with them. This sample demonstrates a multi-tenant MVC web app that allows two types of users – individual and business (team) – to setup account and perform CRUD operations related to contacts and interactions in isolation (each account is only limited to seeing and modifying data that belongs to it).

It incorporates security controls such as [cookie-based authentication](https://ASPSecurityKit.net/features/#auth-cookie), [email verification](https://ASPSecurityKit.net/features/#user-verification), [XSS](https://ASPSecurityKit.net/features/#xss) and so on.

It employs ASPSecurityKit's revolutionary [Activity-Data Authorization (ADA)](https://ASPSecurityKit.net/features/#ada) component for authorization to ensure that each user within the system can only perform operation and access data that belongs to her.

The sample source code is built using the ASK's [MVC template](https://ASPSecurityKit.net/docs/project-templates/#mvc).

Live demo
--------------------

Visit [https://SuperCRM-Mvc.ASPSecurityKit.net](https://SuperCRM-Mvc.ASPSecurityKit.net/) to play with a live demo based on this sample.

Step-by-step tutorial
--------------------

A [step-by-step tutorial](https://ASPSecurityKit.net/docs/getting-started/build-crm-web-application-on-aspdotnet-core-mvc/) is also available, which takes you through 'building SuperCRM from scratch' and in the process, you learn important security related concepts to help you master ASPSecurityKit to rapidly build reliable and secure web applications.

Running the sample
--------------------

### Prerequisits
* [Visual Studio](https://visualstudio.microsoft.com/) 2019 or higher
* [SQL Server Express](https://www.microsoft.com/en-in/sql-server/sql-server-downloads) (built with v14, but latest should work)

### Steps:
* First, clone this repo or download it as zip file.
* Open SuperCRM.sln from the appropriate step (step5 contains the full sample source code) in Visual Studio 2019 or higher.
* From solution explorer, Open appsettings.json and,
    - Make sure that the connectionString for the SuperCRM database is valid for your machine.
    * For email related features to work, you need to set SMTP credentials in the method `SendVerificationMailAsync` method in the file `Controllers\UserController.cs` of some Gmail account (or any other SMTP service you prefer).
* [Generate a trial license](https://aspsecuritykit.net/docs/using-the-aspsecuritykit.tools/#generate-trial-key) because step5 contains more than 15 operations allowed by the [default restrictions](https://aspsecuritykit.net/docs/license/#evaluation-restrictions). On the other hand, a [trial key](https://ASPSecurityKit.net/docs/license/#trial-key) allows far greater number of operations but is valid for a month.
* In Package Manager Console, execute the following command to create the database:
```ps1
update-database
```
* Press F5 to run in debug mode.
    - This sample can only be run on localhost:&lt;port&gt; host-based URL as per the [trial license](#user-content-trial-license).

Feedback
--------------------

Feedback is much appreciated. For any question, to report any issue or need some help, feel free to get in touch on [support@ASPSecurityKit.net](mailto:support@ASPSecurityKit.net)

License
--------------------

This sample source code is licensed under [KHOSLA TECH - END USER AGREEMENT](https://aspsecuritykit.net/legal/end-user-agreement/)

You're free to learn and incorporate techniques and logic available in this sample in projects that uses a [licensed](https://aspsecuritykit.net/docs/license/#license-key) version of ASPSecurityKit.


<a name="trial-license"></a>The [trial license key](https://ASPSecurityKit.net/docs/license/#trial-key) (if any) in the sample lets you run the sample in Visual Studio. It's only meant for this sample and cannot be used for any other project.
