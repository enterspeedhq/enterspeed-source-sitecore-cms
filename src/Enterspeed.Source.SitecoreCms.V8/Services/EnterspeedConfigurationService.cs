using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Sites;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedConfigurationService : IEnterspeedConfigurationService
    {
        private readonly BaseLanguageManager _languageManager;
        private readonly BaseItemManager _itemManager;
        private readonly BaseLinkManager _linkManager;
        private readonly BaseFactory _factory;
        private readonly BaseSiteContextFactory _siteContextFactory;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;

        private List<EnterspeedSitecoreConfiguration> _configuration;
        private DateTime _lastUpdatedDate = DateTime.MinValue;

        public EnterspeedConfigurationService(
            BaseLanguageManager languageManager,
            BaseItemManager itemManager,
            BaseLinkManager linkManager,
            BaseFactory factory,
            BaseSiteContextFactory siteContextFactory,
            IEnterspeedSitecoreLoggingService loggingService)
        {
            _languageManager = languageManager;
            _itemManager = itemManager;
            _linkManager = linkManager;
            _factory = factory;
            _siteContextFactory = siteContextFactory;
            _loggingService = loggingService;
        }

        public List<EnterspeedSitecoreConfiguration> GetConfigurations()
        {
            var enterspeedConfigurationItem = _itemManager.GetItem(EnterspeedIDs.Items.EnterspeedConfigurationID, Language.Parse("en"), Version.Latest, _factory.GetDatabase("web"));
            if (HasNoConfigurationSetUp(enterspeedConfigurationItem))
            {
                return new List<EnterspeedSitecoreConfiguration>();
            }

            if (!IsConfigurationUpdated(enterspeedConfigurationItem, out DateTime _updatedDate))
            {
                return _configuration;
            }

            _configuration = new List<EnterspeedSitecoreConfiguration>();

            foreach (Item enterspeedSiteConfigurationItem in enterspeedConfigurationItem.Children)
            {
                var config = new EnterspeedSitecoreConfiguration
                {
                    IsEnabled = true
                };

                config.IsPreview = enterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedEnablePreviewFieldID] == "1";

                var configApiBaseUrl = enterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedApiBaseUrlFieldID];

                config.BaseUrl = (configApiBaseUrl ?? string.Empty).Trim();

                var configApiKey = enterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedApiKeyFieldID];
                config.ApiKey = (configApiKey ?? string.Empty).Trim();

                config.ItemNotFoundUrl = GetItemNotFoundUrl();

                MultilistField enabledSitesField = enterspeedSiteConfigurationItem.Fields[EnterspeedIDs.Fields.EnterspeedEnabledSitesFieldID];
                MultilistField dictionariesItemPaths = enterspeedSiteConfigurationItem.Fields[EnterspeedIDs.Fields.EnterspeedEnabledDictionariesFieldID];
                var dictionariesItemPathsList = dictionariesItemPaths?.GetItems()?.Select(d => d.Paths.FullPath)?.ToList();

                var enabledSites = enabledSitesField?.GetItems()?.ToList() ?? new List<Item>();
                if (enabledSites.Any())
                {
                    var allSiteInfos = _siteContextFactory.GetSites();

                    foreach (var enabledSite in enabledSites)
                    {
                        var matchingSites = allSiteInfos.Where(x => x.RootPath.Equals(enabledSite.Paths.FullPath, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (!matchingSites.Any())
                        {
                            continue;
                        }

                        foreach (var matchingSite in matchingSites)
                        {
                            var siteContext = _siteContextFactory.GetSiteContext(matchingSite.Name);
                            var siteLanguage = _languageManager.GetLanguage(siteContext.Language);

                            var languageEnterspeedSiteConfigurationItem = enterspeedSiteConfigurationItem.Database.GetItem(enterspeedSiteConfigurationItem.ID, siteLanguage);
                            if (languageEnterspeedSiteConfigurationItem.Versions.Count == 0)
                            {
                                languageEnterspeedSiteConfigurationItem = enterspeedSiteConfigurationItem;
                            }

                            var homeItem = _itemManager.GetItem(siteContext.StartPath, siteLanguage, Version.Latest, siteContext.Database);
                            if (homeItem == null || homeItem.Versions.Count == 0)
                            {
                                _loggingService.Error($"HomeItem is null for site being configured. Site with start path {siteContext.StartPath} and language {siteLanguage}");
                                continue;
                            }

                            var siteBaseUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedSiteBaseUrlFieldID];
                            var siteMediaBaseUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedMediaBaseUrlFieldID];
                            var publishHookUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedpublishHookUrlFieldID];

                            var name = siteContext.SiteInfo.Name;
                            string startPathUrl;
                            using (var siteContextSwitcher = new SiteContextSwitcher(siteContext))
                            {
                                startPathUrl = _linkManager.GetItemUrl(homeItem, new UrlOptions
                                {
                                    SiteResolving = true,
                                    Site = siteContext,
                                    AlwaysIncludeServerUrl = true,
                                    LowercaseUrls = true,
                                    LanguageEmbedding = LanguageEmbedding.Never
                                });
                            }

                            var enterspeedSiteInfo = new EnterspeedSiteInfo
                            {
                                Name = name,
                                StartPathUrl = startPathUrl,
                                BaseUrl = siteBaseUrl,
                                MediaBaseUrl = siteMediaBaseUrl,
                                PublishHookUrl = publishHookUrl,
                                HomeItemPath = siteContext.StartPath,
                                SiteItemPath = siteContext.RootPath,
                                Language = siteLanguage.Name,
                                DictionariesItemPaths = dictionariesItemPathsList
                            };

                            if (siteContext.Properties["scheme"] != null &&
                                siteContext.Properties["scheme"].Equals("https", StringComparison.OrdinalIgnoreCase))
                            {
                                enterspeedSiteInfo.IsHttpsEnabled = true;
                            }

                            config.SiteInfos.Add(enterspeedSiteInfo);
                        }
                    }
                }

                // Settings caching values
                _configuration.Add(config);
                _lastUpdatedDate = _updatedDate;
            }

            return _configuration;
        }

        private static string GetItemNotFoundUrl()
        {
            var url = Settings.ItemNotFoundUrl;
            if (string.IsNullOrEmpty(url))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed API Key from the Sitecore Setting \"ItemNotFoundUrl\".");
            }

            return url;
        }
        private bool HasNoConfigurationSetUp(Item enterspeedConfigurationItem)
        {
            return enterspeedConfigurationItem == null
                || enterspeedConfigurationItem.Versions.Count == 0
                || enterspeedConfigurationItem.Children == null
                || !enterspeedConfigurationItem.Children.Any();
        }

        private bool IsConfigurationUpdated(Item item, out DateTime currentUpdatedDate)
        {
            currentUpdatedDate = item.Children.Max(i => i.Statistics.Updated);

            if (_lastUpdatedDate >= currentUpdatedDate &&
                _configuration != null)
            {
                return false;
            }

            return true;
        }
    }
}