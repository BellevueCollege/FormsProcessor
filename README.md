FormsProcessor
==============

A 'form mailer' script for **simple** HTML forms, with additional security and VERY basic routing features. This **IS NOT** a document management tool. For more detailed information, see the [project wiki](https://github.com/BellevueCollege/FormsProcessor/wiki)

## About the project(s)

### Files & folders

* _FormProcessor.sln_ - Visual Studio solution file; opening this will load all projects, etc.
  * `FormProcessor.DB` - Database project, using [Red-Gate Tools' SQL Connect](https://www.red-gate.com/products/sql-development/sql-connect/).
  * `FormProcessor.Web` - ASP.NET Web Forms project.
    * `configs` - Example configuration files - you will need to create your own in this folder.
  * `packages` - 3rd-party packages added via [NuGet](https://nuget.codeplex.com/).

### Requirements

* Visual Studio 2012
* Microsoft .NET Framwork 4 (Mono is not supported at this time, but is a future possibility)
* Microsoft SQL Server 2008 - not strictly required, but this is the product the project was built and tested with.

### 3rd-party libraries

The following libraries have already been included in the project (via [NuGet](https://nuget.codeplex.com/)):

* AntiXSS
* Common.Logging
  * Elmah
  * NLog
* ELMAH
* EntityFramework

## About the database project & files

The database files are managed in a project template for Visual Studio provided by [Red-Gate](https://www.red-gate.com/)'s _SQL Connect_ tool. **This is a commercial product**. If you do not have _SQL Connect_, Visual Studio will report an error saying it is unable to open this project type.

The database files are actually stored in plain-text SQL scripts, in a reasonable folder structure, however. So those without _SQL Connect_ should still be able to make use of them.
