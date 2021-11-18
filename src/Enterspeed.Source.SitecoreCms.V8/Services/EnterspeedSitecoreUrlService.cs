using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Sites;

namespace Enterspeed.Source.SitecoreCms.V8.Services
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

        public string GetItemUrl(Item item, EnterspeedSitecoreConfiguration configuration, bool enableLanguageEmbedding = false)
        {
            if (item == null)
            {
                return null;
            }

            EnterspeedSiteInfo siteInfo = configuration.GetSite(item);

                var options = new UrlOptions
            {
                SiteResolving = true,
                AlwaysIncludeServerUrl = true,
                LowercaseUrls = true,
                LanguageEmbedding = enableLanguageEmbedding ? LanguageEmbedding.Always : LanguageEmbedding.Never
            };

            if (siteInfo != null)
            {
                SiteContext siteContext = _siteContextFactory.GetSiteContext(siteInfo.Name);

                options.Site = siteContext;
            }
            using (var siteContextSwitcher = new SiteContextSwitcher(options.Site))
            {
                return options.Site.Properties["scheme"]??"http"+ LinkManager.GetItemUrl(item, options);
            }
        }

        public string GetMediaUrl(MediaItem mediaItem)
        {
            if (mediaItem == null)
            {
                return null;
            }

            var urlBuilderOptions = new MediaUrlOptions
            {
                AbsolutePath = true,
                AlwaysIncludeServerUrl = false
            };

            string mediaUrl = MediaManager.GetMediaUrl(mediaItem, urlBuilderOptions);

            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            if (mediaUrl.EndsWith(".ashx") &&
                !string.IsNullOrEmpty(mediaItem.Extension))
            {
                mediaUrl = mediaUrl.Replace(".ashx", $".{mediaItem.Extension}");
            }

            return mediaUrl;
        }
    }
}