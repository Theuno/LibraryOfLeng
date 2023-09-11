# Leng
Magic: The Gathering (MTG) is more than just a card game; it's a world of strategy, skill, and endless possibilities. If you're a dedicated geek and MTG enthusiast, you understand the thrill of building decks, collecting cards, and mastering the game's intricacies. However, to truly enhance your MTG journey, you need a tool that goes beyond the basics. That's where Leng comes in.

## The Ultimate Collection Manager:

Leng provides a comprehensive collection manager that lets you effortlessly catalog your entire card collection. From your prized rares to those hidden gems, Leng keeps track of every card.
Advanced search and filtering options help you quickly find the cards you need for your next deck, ensuring that you're always one step ahead of your opponents.

Join the Leng community today and take your Magic: The Gathering adventure to new heights!


# Technical Details
Are you ready to delve into the technical wizardry that powers Leng? As a dedicated geek and Magic: The Gathering enthusiast, you'll appreciate the meticulous craftsmanship that goes into creating this powerful tool. Here's a peek behind the scenes:

## Cutting-Edge Technologies:

* C# and .NET Core: Leng's back-end is built on C# and .NET Core, providing a solid foundation for robust and efficient performance. .NET Core's cross-platform capabilities ensure Leng runs smoothly on various operating systems.
* Entity Framework Core: We harness the power of Entity Framework Core to work with our database seamlessly. This ORM (Object-Relational Mapping) framework simplifies data access and management.
* Blazor: Leng's front-end is crafted using Blazor, a modern web framework that allows us to create interactive and dynamic user interfaces with C# and Razor components. No need to rely solely on JavaScript—C# is our go-to language for front-end magic.

## Community-Driven Development:

* GitHub: Leng's development thrives on collaboration. Our codebase is hosted on GitHub, making it easy for fellow geeks and MTG players to contribute, report issues, and suggest enhancements.

## Continuous Integration and Deployment:

* Azure Pipelines: Leng's development pipeline is powered by Azure Pipelines, which allows us to automate our build and deployment processes. This ensures that our codebase is always up-to-date and ready to go.
## High-Quality Code:
* SonarCloud: We use SonarCloud to maintain our code quality and ensure that our codebase is clean, concise, and easy to maintain. SonarCloud also helps us catch bugs and vulnerabilities early on, so we can fix them before they become a problem.
## Automated Testing:
* xUnit: Leng's unit tests are built using xUnit, a popular unit testing framework for .NET Core applications. This allows us to test our code and ensure that everything works as expected.

# Development
## Prerequisites
* .NET Core SDK 7+
* Visual Studio 2022 or later with the ASP.NET and web development workload
* SQL Server LocalDB

## Clone the repository
```
git clone
```

## Initialize the database
To get started, the database connection string must be updated in the appsettings.json file. The default connection string is configured to use SQL Server LocalDB. If you have a different database server, you can update the connection string accordingly.

Next to initialize the database, run the following command in the Package Manager Console:
```
dotnet ef database update --project Leng.Infrastructure --startup-project Leng.BlazorServer
```

## Apply migrations:
Open the Package Manager Console and run the following command:
```
dotnet ef database update --project Leng.Infrastructure --startup-project Leng.BlazorServer
```
