# Background email sender sample
This is a contact form app. E-mail messages are sent in background by using an **ASP.NET Core Hosted Service** in a fire-and-forget fashion.

Check out the actual Hosted Service implementation in the code file [HostedServices/EmailSenderHostedService.cs](HostedServices/EmailSenderHostedService.cs).

## Warning
This code is **NOT production-ready**. It just illustrates how to implement and register a Hosted Service for sending e-mails. Some messages might be lost since this webapp does not implement a retry mechanism nor it uses a persistent storage.

## Getting started
Edit the [appsettings.json](appsettings.json) file with your SMTP server data. Then, just run the application by typing `dotnet run`. The .NET Core SDK 2.0 (or greater) must be installed in your system. Fill in the form and hit the Send button.
![home.png](home.png)
