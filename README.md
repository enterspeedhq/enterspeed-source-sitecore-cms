# [Enterspeed Sitecore Source](https://www.enterspeed.com/) &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE) [![NuGet version](https://img.shields.io/nuget/v/Enterspeed.Source.SitecoreCms.V9)](https://www.nuget.org/packages/Enterspeed.Source.SitecoreCms.V9/) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/enterspeedhq/enterspeed-source-sitecore-cms/pulls) [![Twitter](https://img.shields.io/twitter/follow/enterspeedhq?style=social)](https://twitter.com/enterspeedhq)

## Installation

To get started with the Enterspeed Sitecore integration you should follow these steps:

### Install the NuGet package

Using the Package Manager

```powershell
Install-Package Enterspeed.Source.SitecoreCms.V9 -Version <version>
```

The NuGet package installs config files into this directory; verify that this folder contains config files

```powershell
~\App_Config\Include\Enterspeed
```

### Configuring Enterspeed in Sitecore

Once installed, your Sitecore instance will be loaded with a new item in ```/sitecore/system``` called "Enterspeed Configuration".

You will have to enter the necessary values in the respective fields, and publish the changes.

## How it works

This connector revolves a lot around the setting above, EnabledSites. The philosophy is that you can publish every item in your Sitecore solution and only items that belong to your enabled sites are being ingested to/deleted in Enterspeed.

### Site

This connector requires a Site/Home item structure in ```/sitecore/content```, which could look like this:

* YourSite <-- add this item to the EnabledSites configuration field
  * Home

### Content

Content items that are being sent to Enterspeed, will have references to the renderings inserted on them, along with information of the fields of the given datasources inserted on these renderings.

Each rendering reference sent to Enterspeed could have these properties:

* ```renderingId``` - the Enterspeed ID for this rendering
* ```renderingPlaceholder``` - the Sitecore placeholder inserted on either the presentation details or the rendering itself
* ```renderingParameters``` - an array of key/values inserted on the rendering options
* ```renderingDatasource``` - a reference to the inserted datasource item

### Renderings

Renderings are processed separately, as well, but only if the rendering is inserted on the presentation details of a content item which resides in an enabled site. This means that newly created renderings are not processed until they're inserted to be rendered on content for which you have enabled.

### Supported field types

* Single-Line Text
* Rich-Text
* Checkbox
* Date
* File
* Image
* Integer
* Multi-Line Text
* Number
* Checklist
* Droplist
* Grouped Droplink
* Grouped Droplist
* Multilist
* Name Value List
* Name Lookup Value List
* Treelist
* Droplink
* Droptree
* General Link

### Debugging

You can debug what is sent to Enterspeed when publishing Sitecore content, by requesting this path:

* ```/api/sitecore/enterspeed/debug?id={idOrFullPath}```

## Roadmap

* Improved logging.
* Functionality to trigger a push to Enterspeed from a contextual ribbon/action in Sitecore.
* Forms support.
* TBD.

## Contributing

Pull requests are very welcome.  
Please fork this repository and make a PR when you are ready.  

Otherwise you are welcome to open an Issue in our [issue tracker](https://github.com/enterspeedhq/enterspeed-source-sitecore-cms/issues).

## License

Enterspeed Sitecore Source is [MIT licensed](./LICENSE)
