using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Web;
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

        private List<EnterspeedSitecoreConfiguration> _configuration;
        private Guid _configurationRevisionId = Guid.Empty;

        private DateTime _lastUpdatedDate = DateTime.MinValue;

        public EnterspeedConfigurationService(
            BaseLanguageManager languageManager,
            BaseItemManager itemManager,
            BaseLinkManager linkManager,
            BaseFactory factory,
            BaseSiteContextFactory siteContextFactory)
        {
            _languageManager = languageManager;
            _itemManager = itemManager;
            _linkManager = linkManager;
            _factory = factory;
            _siteContextFactory = siteContextFactory;
        }

        public List<EnterspeedSitecoreConfiguration> GetConfiguration()
        {
            Item enterspeedConfigurationItem = _itemManager.GetItem(EnterspeedIDs.Items.EnterspeedConfigurationID, Language.Parse("en"), Version.Latest, _factory.GetDatabase("web"));
            if (enterspeedConfigurationItem == null || enterspeedConfigurationItem.Versions.Count == 0)
            {
                return new List<EnterspeedSitecoreConfiguration>();
            }

            //string enabled = enterspeedConfigurationItem[EnterspeedIDs.Fields.EnterspeedEnabledFieldID] ?? string.Empty;
            //if (enabled != "1")
            //{
            //    return new List<EnterspeedSitecoreConfiguration>();
            //}
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

                string configApiBaseUrl = enterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedApiBaseUrlFieldID];

                config.BaseUrl = (configApiBaseUrl ?? string.Empty).Trim();

                string configApiKey = enterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedApiKeyFieldID];
                config.ApiKey = (configApiKey ?? string.Empty).Trim();

                config.ItemNotFoundUrl = GetItemNotFoundUrl();

                MultilistField enabledSitesField = enterspeedSiteConfigurationItem.Fields[EnterspeedIDs.Fields.EnterspeedEnabledSitesFieldID];

                var enabledSites = enabledSitesField?.GetItems()?.ToList() ?? new List<Item>();
                if (enabledSites.Any())
                {
                    List<SiteInfo> allSiteInfos = _siteContextFactory.GetSites();

                    foreach (Item enabledSite in enabledSites)
                    {
                        var matchingSites = allSiteInfos.Where(x => x.RootPath.Equals(enabledSite.Paths.FullPath, StringComparison.OrdinalIgnoreCase));
                        if (matchingSites.Count() == 0)
                        {
                            continue;
                        }

                        foreach (var matchingSite in matchingSites)
                        {
                            SiteContext siteContext = _siteContextFactory.GetSiteContext(matchingSite.Name);

                            Language siteLanguage = _languageManager.GetLanguage(siteContext.Language);

                            var languageEnterspeedSiteConfigurationItem = enterspeedSiteConfigurationItem.Database.GetItem(enterspeedSiteConfigurationItem.ID, siteLanguage);
                            if (languageEnterspeedSiteConfigurationItem.Versions.Count == 0)
                            {
                                languageEnterspeedSiteConfigurationItem = enterspeedSiteConfigurationItem;
                            }

                            Item homeItem = _itemManager.GetItem(siteContext.StartPath, siteLanguage, Version.Latest, siteContext.Database);

                            if (homeItem == null || homeItem.Versions.Count == 0)
                            {
                                // TODO - KEK: throw exception here?
                                continue;
                            }

                            string siteBaseUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedSiteBaseUrlFieldID];
                            string siteMediaBaseUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedMediaBaseUrlFieldID];
                            string publishHookUrl = languageEnterspeedSiteConfigurationItem[EnterspeedIDs.Fields.EnterspeedpublishHookUrlFieldID];

                            string name = siteContext.SiteInfo.Name;
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
                            if (siteContext.Properties["scheme"] == null)
                            {
                                startPathUrl = "http" + startPathUrl;
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
                                Language = siteLanguage.Name
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

        private static string GetItemNotFoundUrl( )
        {
            string url = Settings.ItemNotFoundUrl;
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

        private bool IsConfigurationUpdated(Item item, out DateTime currentUpdatedDate)
        {
            currentUpdatedDate = item.Children.Max(i=> i.Statistics.Updated);

            if (_lastUpdatedDate >= currentUpdatedDate &&
                _configuration != null)
            {
                return false;
            }

            return true;
        }
    }
}