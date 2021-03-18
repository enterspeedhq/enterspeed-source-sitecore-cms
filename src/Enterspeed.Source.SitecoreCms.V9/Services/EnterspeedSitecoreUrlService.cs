using System;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;
using Sitecore.Sites;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedSitecoreUrlService : IEnterspeedUrlService
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly BaseSiteContextFactory _siteContextFactory;
        private readonly BaseLinkManager _linkManager;

        public EnterspeedSitecoreUrlService(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            BaseSiteContextFactory siteContextFactory,
            BaseLinkManager linkManager)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _siteContextFactory = siteContextFactory;
            _linkManager = linkManager;
        }

        public string GetItemUrl(Item item, bool enableLanguageEmbedding = false)
        {
            EnterspeedSitecoreConfiguration configuration = _enterspeedConfigurationService.GetConfiguration();
            EnterspeedSiteInfo siteInfo = configuration.GetSite(item);

            var urlBuilderOptions = new ItemUrlBuilderOptions
            {
                SiteResolving = true,
                AlwaysIncludeServerUrl = true,
                LowercaseUrls = true,
                LanguageEmbedding = enableLanguageEmbedding ? LanguageEmbedding.Always : LanguageEmbedding.Never
            };

            if (siteInfo != null)
            {
                SiteContext siteContext = _siteContextFactory.GetSiteContext(siteInfo.Name);

                urlBuilderOptions.Site = siteContext;
            }

            return _linkManager.GetItemUrl(item, urlBuilderOptions);
        }
    }
}