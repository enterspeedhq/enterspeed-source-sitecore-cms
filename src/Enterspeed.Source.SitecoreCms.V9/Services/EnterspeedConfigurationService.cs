using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V9.Exceptions;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Sites;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedConfigurationService : IEnterspeedConfigurationService
    {
        private readonly BaseSettings _settings;
        private readonly BaseLanguageManager _languageManager;
        private readonly BaseItemManager _itemManager;
        private readonly BaseLinkManager _linkManager;
        private readonly BaseSiteContextFactory _siteContextFactory;

        private EnterspeedSitecoreConfiguration _configuration;

        public EnterspeedConfigurationService(
            BaseSettings settings,
            BaseLanguageManager languageManager,
            BaseItemManager itemManager,
            BaseLinkManager linkManager,
            BaseSiteContextFactory siteContextFactory)
        {
            _settings = settings;
            _languageManager = languageManager;
            _itemManager = itemManager;
            _linkManager = linkManager;
            _siteContextFactory = siteContextFactory;
        }

        public EnterspeedSitecoreConfiguration GetConfiguration()
        {
            if (_configuration != null)
            {
                return _configuration;
            }

            var configuration = new EnterspeedSitecoreConfiguration
            {
                BaseUrl = GetEnterspeedBaseUrl(_settings),
                ApiKey = GetEnterspeedApiKey(_settings),
                ItemNotFoundUrl = GetItemNotFoundUrl(_settings)
            };

            foreach (SiteContext siteContext in GetEnterspeedEnabledSites(_settings))
            {
                Language siteLanguage = _languageManager.GetLanguage(siteContext.Language);

                Item homeItem = _itemManager.GetItem(siteContext.StartPath, siteLanguage, Version.Latest, siteContext.Database);
                if (homeItem == null || homeItem.Versions.Count == 0)
                {
                    // TODO - KEK: throw exception here?
                    continue;
                }

                string name = siteContext.SiteInfo.Name;
                string startPathUrl = _linkManager.GetItemUrl(homeItem, new ItemUrlBuilderOptions
                {
                    SiteResolving = true,
                    Site = siteContext,
                    AlwaysIncludeServerUrl = true,
                    LowercaseUrls = true,
                    LanguageEmbedding = LanguageEmbedding.Never
                });

                var enterspeedSiteInfo = new EnterspeedSiteInfo
                {
                    Name = name,
                    BaseUrl = startPathUrl,
                    HomeItemPath = siteContext.StartPath // StartPath combines RootPath and StartItem
                };

                if (siteContext.Properties["scheme"] != null &&
                    siteContext.Properties["scheme"].Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    enterspeedSiteInfo.IsHttpsEnabled = true;
                }

                configuration.SiteInfos.Add(enterspeedSiteInfo);
            }

            _configuration = configuration;

            return _configuration;
        }

        private static string GetEnterspeedBaseUrl(BaseSettings settings)
        {
            string baseUrl = settings.GetSetting("Enterspeed.BaseUrl", null);
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed Base Url from the Sitecore Setting \"Enterspeed.BaseUrl\".");
            }

            return baseUrl;
        }

        private static string GetEnterspeedApiKey(BaseSettings settings)
        {
            string apiKey = settings.GetSetting("Enterspeed.ApiKey", null);
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed API Key from the Sitecore Setting \"Enterspeed.ApiKey\".");
            }

            return apiKey;
        }

        private static string GetItemNotFoundUrl(BaseSettings settings)
        {
            string url = settings.GetSetting("ItemNotFoundUrl", null);
            if (string.IsNullOrEmpty(url))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed API Key from the Sitecore Setting \"ItemNotFoundUrl\".");
            }

            return url;
        }

        private List<SiteContext> GetEnterspeedEnabledSites(BaseSettings settings)
        {
            string enabledSites = settings.GetSetting("Enterspeed.EnabledSites", null);
            if (string.IsNullOrEmpty(enabledSites))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed Enabled Sites from the Sitecore Setting \"Enterspeed.EnabledSites\".");
            }

            List<SiteContext> sites = enabledSites
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(_siteContextFactory.GetSiteContext)
                .ToList();

            if (sites.Any() == false)
            {
                throw new EnterspeedSitecoreException(
                    "No sites seem to be configured in the Sitecore Setting \"Enterspeed.EnabledSites\".");
            }

            return sites;
        }
    }
}