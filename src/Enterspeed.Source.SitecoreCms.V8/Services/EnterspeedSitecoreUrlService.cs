using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Sites;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedSitecoreUrlService : IEnterspeedUrlService
    {
        private readonly BaseSiteContextFactory _siteContextFactory;
        private readonly BaseMediaManager _mediaManager;

        public EnterspeedSitecoreUrlService(
            BaseSiteContextFactory siteContextFactory,
            BaseMediaManager mediaManager)
        {
            _siteContextFactory = siteContextFactory;
            _mediaManager = mediaManager;
        }

        public string GetItemUrl(Item item, EnterspeedSiteInfo siteInfo, bool enableLanguageEmbedding = false)
        {
            if (item == null)
            {
                return null;
            }

            var urlBuilderOptions = UrlOptions.DefaultOptions;
            urlBuilderOptions.Language = item.Language;
            urlBuilderOptions.SiteResolving = true;
            urlBuilderOptions.AlwaysIncludeServerUrl = true;
            urlBuilderOptions.LowercaseUrls = true;
            urlBuilderOptions.LanguageEmbedding = enableLanguageEmbedding ? LanguageEmbedding.Always : LanguageEmbedding.Never;

            if (siteInfo != null)
            {
                SiteContext siteContext = _siteContextFactory.GetSiteContext(siteInfo.Name);

                urlBuilderOptions.Site = siteContext;
                urlBuilderOptions.SiteResolving = string.IsNullOrEmpty(siteInfo.BaseUrl);
                urlBuilderOptions.AlwaysIncludeServerUrl = string.IsNullOrEmpty(siteInfo.BaseUrl);
            }

            using (var siteContextSwitcher = new SiteContextSwitcher(urlBuilderOptions.Site))
            {
                var itemUrl = LinkManager.GetItemUrl(item, urlBuilderOptions);
                if (!string.IsNullOrEmpty(siteInfo.BaseUrl))
                {
                    itemUrl = siteInfo.BaseUrl + itemUrl.Replace(siteInfo.StartPathUrl, "/");
                }
                return itemUrl;
            }


        }

        public string GetMediaUrl(MediaItem mediaItem, EnterspeedSiteInfo siteInfo)
        {
            if (mediaItem == null)
            {
                return null;
            }

            var urlBuilderOptions = new MediaUrlOptions()
            {
                AbsolutePath = true,
                AlwaysIncludeServerUrl = string.IsNullOrEmpty(siteInfo.MediaBaseUrl),
                Language = Language.Invariant
            };

            string mediaUrl = _mediaManager.GetMediaUrl(mediaItem, urlBuilderOptions);
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(siteInfo.MediaBaseUrl))
            {
                mediaUrl = siteInfo.MediaBaseUrl + mediaUrl;
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