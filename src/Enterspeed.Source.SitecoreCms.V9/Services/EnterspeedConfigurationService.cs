using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V9.Exceptions;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Sites;
using Sitecore.Web;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedConfigurationService : IEnterspeedConfigurationService
    {
        private readonly BaseSettings _settings;
        private readonly BaseLanguageManager _languageManager;
        private readonly BaseItemManager _itemManager;
        private readonly BaseLinkManager _linkManager;
        private readonly BaseFactory _factory;
        private readonly BaseSiteContextFactory _siteContextFactory;

        private EnterspeedSitecoreConfiguration _configuration;
        private Guid _configurationRevisionId = Guid.Empty;

        public EnterspeedConfigurationService(
            BaseSettings settings,
            BaseLanguageManager languageManager,
            BaseItemManager itemManager,
            BaseLinkManager linkManager,
            BaseFactory factory,
            BaseSiteContextFactory siteContextFactory)
        {
            _settings = settings;
            _languageManager = languageManager;
            _itemManager = itemManager;
            _linkManager = linkManager;
            _factory = factory;
            _siteContextFactory = siteContextFactory;
        }

        public EnterspeedSitecoreConfiguration GetConfigurationFromSitecore()
        {
            Item enterspeedConfigurationItem = _itemManager.GetItem(EnterspeedIDs.Items.EnterspeedConfigurationID, Language.Parse("en"), Version.Latest, _factory.GetDatabase("web"));
            if (enterspeedConfigurationItem == null || enterspeedConfigurationItem.Versions.Count == 0)
            {
                throw new NullReferenceException("Unable to find Enterspeed Configuration item.");
            }

            if (IsConfigurationUpdated(enterspeedConfigurationItem, out Guid currentRevisionId) == false)
            {
                return _configuration;
            }

            var config = new EnterspeedSitecoreConfiguration();

            string configApiBaseUrl = enterspeedConfigurationItem[EnterspeedIDs.Templates.Fields.EnterspeedApiBaseUrlFieldID];
            config.BaseUrl = (configApiBaseUrl ?? string.Empty).Trim();

            string configApiKey = enterspeedConfigurationItem[EnterspeedIDs.Templates.Fields.EnterspeedApiKeyFieldID];
            config.ApiKey = (configApiKey ?? string.Empty).Trim();

            config.ItemNotFoundUrl = GetItemNotFoundUrl(_settings);

            MultilistField enabledSitesField = enterspeedConfigurationItem.Fields[EnterspeedIDs.Templates.Fields.EnterspeedEnabledSitesFieldID];

            var enabledSites = enabledSitesField?.GetItems()?.ToList() ?? new List<Item>();
            if (enabledSites.Any())
            {
                List<SiteInfo> allSiteInfos = _siteContextFactory.GetSites();

                foreach (Item enabledSite in enabledSites)
                {
                    var matchingSite = allSiteInfos.FirstOrDefault(x => x.RootPath.Equals(enabledSite.Paths.FullPath, StringComparison.OrdinalIgnoreCase));
                    if (matchingSite == null)
                    {
                        continue;
                    }

                    SiteContext siteContext = _siteContextFactory.GetSiteContext(matchingSite.Name);

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
                        SiteItemPath = siteContext.RootPath
                    };

                    if (siteContext.Properties["scheme"] != null &&
                        siteContext.Properties["scheme"].Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        enterspeedSiteInfo.IsHttpsEnabled = true;
                    }

                    config.SiteInfos.Add(enterspeedSiteInfo);
                }
            }

            // Settings caching values
            _configuration = config;
            _configurationRevisionId = currentRevisionId;

            return _configuration;
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

        private bool IsConfigurationUpdated(Item item, out Guid currentRevisionId)
        {
            currentRevisionId = Guid.Parse(item.Statistics.Revision);

            if (_configurationRevisionId == currentRevisionId &&
                _configuration != null)
            {
                return false;
            }

            return true;
        }
    }
}