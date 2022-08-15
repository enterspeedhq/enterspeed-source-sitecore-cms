using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultImageFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private const string PropertyAlt = "alt";
        private const string PropertyUrl = "url";
        private const string PropertyExtension = "extension";
        private const string PropertySize = "size";
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedUrlService _urlService;

        public DefaultImageFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedUrlService urlService)
        {
            _fieldService = fieldService;
            _urlService = urlService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("image", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            ImageField imageField = field;
            if (imageField?.MediaItem == null)
            {
                return null;
            }

            var properties = new Dictionary<string, IEnterspeedProperty>();

            if (!string.IsNullOrEmpty(imageField.Alt))
            {
                properties.Add(PropertyAlt, new StringEnterspeedProperty(PropertyAlt, imageField.Alt));
            }

            if (!string.IsNullOrEmpty(imageField.MediaItem.Fields["Size"].Value))
            {
                properties.Add(PropertySize, new StringEnterspeedProperty(PropertySize, imageField.MediaItem.Fields["Size"].Value));
            }

            if (!string.IsNullOrEmpty(imageField.MediaItem.Fields["Extension"].Value))
            {
                properties.Add(PropertyExtension, new StringEnterspeedProperty(PropertyExtension, imageField.MediaItem.Fields["Extension"].Value));
            }

            string mediaUrl = _urlService.GetMediaUrl(imageField.MediaItem, siteInfo);
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            properties.Add(PropertyUrl, new StringEnterspeedProperty(PropertyUrl, mediaUrl));
            return new ObjectEnterspeedProperty(_fieldService.GetFieldName(field), properties);
        }
    }
}