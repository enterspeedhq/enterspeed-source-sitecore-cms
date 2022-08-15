# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project adheres to Semantic Versioning.

## [0.9.0 - 2021-08-15]
* Pushing datasources async instead of sync
* Limited dictionaries to only publish the selected area of the dictionaries
* Ensure that config is only published, when actually updated
* Delete datasources based on comparison between master and web database, instead of publish configuration

## [0.8.0 - 2021-08-10]
* https://github.com/enterspeedhq/enterspeed-source-sitecore-cms/pull/21

## [0.3.0 - 2021-05-20]

* Fixed problem when exceptions are thrown too early from the configuration service.
* Added enabled checkbox on the Enterspeed configuration item.
* Added requirement to ```Enterspeed.Source.Sdk``` v0.4.0 as a minimum.
* Added support for dictionary items to be ingested.
* The ```/api/sitecore/enterspeed/debug``` endpoint can now be used with a GUID, itemPath or Enterspeed ID.
* Field names are now sanitized and prefixed with the section name they reside in.
* Now logs into separate Enterspeed log file.
* The "url" property of an ```SitecoreContentEntity``` is now only mapped if the particular item is routable.
* The "metaData" of the mapped Enterspeed Entity now contains an array of available languages for that particular item.

## [0.2.0 - 2021-03-26]

* Fixed problem with NuGet package dependencies.
* Fixed typo in readme.
* ParentId is now correctly mapped when parent represents the Site item.
* Fixed ingest bug caused by redirects array not set to an instance.
* Changed several field converters to have the correct properties added.
* Moved configuration to Sitecore. Previous <setting /> tags are no longer used.
* Added way of debugging what will be sent to Enterspeed (via requesting '/api/sitecore/enterspeed/debug?id=xyz').
* Fixed potential bug when unpublishing items.

## [0.1.0 - 2021-03-22]

* Initial release