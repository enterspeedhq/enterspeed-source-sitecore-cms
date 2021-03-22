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
        private readonly BaseMediaManager _mediaManager;

        public EnterspeedSitecoreUrlService(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            BaseSiteContextFactory siteContextFactory,
            BaseLinkManager linkManager,
            BaseMediaManager mediaManager)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _siteContextFactory = siteContextFactory;
            _linkManager = linkManager;
            _mediaManager = mediaManager;
        }

        public string GetItemUrl(Item item, bool enableLanguageEmbedding = false)
        {
            if (item == null)
            {
                return null;
            }

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

        public string GetMediaUrl(MediaItem mediaItem)
        {
            if (mediaItem == null)
            {
                return null;
            }

            var urlBuilderOptions = new MediaUrlBuilderOptions
            {
                AbsolutePath = true,
                AlwaysIncludeServerUrl = true,
                LanguageEmbedding = LanguageEmbedding.Never
            };

            string mediaUrl = _mediaManager.GetMediaUrl(mediaItem, urlBuilderOptions);
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            if (mediaUrl.EndsWith(".ashx") &&
                string.IsNullOrEmpty(mediaItem.Extension) == false)
            {
                mediaUrl = mediaUrl.Replace(".ashx", $".{mediaItem.Extension}");
            }

            return mediaUrl;
        }
    }
}