using System;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Links.UrlBuilders;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultFileFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedUrlService _urlService;

        public DefaultFileFieldValueConverter(
            IEnterspeedUrlService urlService)
        {
            _urlService = urlService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("file", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo)
        {
            FileField fileField = field;
            if (fileField?.MediaItem == null)
            {
                return null;
            }

            string mediaUrl = _urlService.GetMediaUrl(fileField.MediaItem);
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            return new StringEnterspeedProperty(field.Name, mediaUrl);
        }
    }
}