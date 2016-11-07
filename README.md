# PipelineCRM

_A CRM for the Umbraco back-office - form workflow, personalisation and more..._

## Requirements

- At least Umbraco 7

## How to install to an existing Umbraco website

**Using NuGet (recommended)**

In Visual Studio, open up the Package Manager Console and type the following:

```Install-Package PipelineCRM <project name>```

**Using NuGet locally**

If you would rather build PipelineCRM locally and install from the created files, follow the build guide below, followed by running the following in the Package Manager Console (in Visual Studio):

```Install-Package PipelineCRM <project name> -source C:\path\to\nupkg\containing\folder```

## How to build

**Requirements**

- Node installed
- NPM installed
- Gulp installed globally (`npm install gulp -g`)
- NuGet CLI installed

**Building**

1. Clone this repository to your local system using git
2. Open the solution with Visual Studio and build
3. Open a terminal in the root of the cloned folder
4. Type `npm install` to get all Gulp dependencies
5. Type `gulp pack` (this moves DLLs and App_Plugins to the NuGet folder), and runs `nuget pack` using the `Package.nuspec` file

## Contributing

If you want to setup this project so you can start editing code in a fast manner, then you can do so using this guide:

1. Install the package to your Umbraco website using NuGet
2. Clone this repository to your local filesystem
3. Open a terminal in the root of the cloned folder
4. Type `npm install` to get all Gulp dependencies
5. Edit the `config.json` to include paths to your Umbraco website
6. Run `gulp`

Running `gulp` will copy App_Plugins and new DLLs to your Umbraco website. This mean that you don't have to keep installing though NuGet, or copying files around your filesystem manually.

