using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultFileFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private const string PropertyExtension = "extension";
        private const string PropertyUrl = "url";
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedUrlService _urlService;

        public DefaultFileFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedUrlService urlService)
        {
            _fieldService = fieldService;
            _urlService = urlService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("file", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            FileField fileField = field;

            var properties = new Dictionary<string, IEnterspeedProperty>();
            if (fileField?.MediaItem == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(fileField.MediaItem.Fields["Extension"].Value))
            {
                properties.Add(PropertyExtension, new StringEnterspeedProperty(PropertyExtension, fileField.MediaItem.Fields["Extension"].Value));
            }

            string mediaUrl = _urlService.GetMediaUrl(fileField.MediaItem, siteInfo);
            if (string.IsNullOrEmpty(mediaUrl))
            {
                return null;
            }

            properties.Add(PropertyUrl, new StringEnterspeedProperty(PropertyUrl, mediaUrl));

            return new ObjectEnterspeedProperty(_fieldService.GetFieldName(field), properties);
        }
    }
}