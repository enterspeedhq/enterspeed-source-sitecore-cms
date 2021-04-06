# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project adheres to Semantic Versioning.

## [0.2.1 - 2021-04-06]

* Fixed problem when exceptions are thrown too early from the configuration service.
* Added enabled checkbox on the Enterspeed configuration item.
* Added requirement to ```Enterspeed.Source.Sdk``` v0.4.0 as a minimum.
* Added support for dictionary items to be ingested.
* The ```/api/sitecore/enterspeed/debug``` endpoint can now be used with a GUID, itemPath or Enterspeed ID.
* Field names are now sanitized and prefixed with the section name they reside in.

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