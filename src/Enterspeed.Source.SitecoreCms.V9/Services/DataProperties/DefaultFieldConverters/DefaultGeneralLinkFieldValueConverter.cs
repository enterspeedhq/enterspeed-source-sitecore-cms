using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links.UrlBuilders;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultGeneralLinkFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedUrlService _urlService;

        public DefaultGeneralLinkFieldValueConverter(
            IEnterspeedUrlService urlService)
        {
            _urlService = urlService;
        }

        public bool CanConvert(Field field)
        {
            return field != null &&
                (field.TypeKey.Equals("general link", StringComparison.OrdinalIgnoreCase) ||
                    field.TypeKey.Equals("general link with search", StringComparison.OrdinalIgnoreCase));
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            LinkField linkField = field;
            if (linkField == null)
            {
                return null;
            }

            var properties = new Dictionary<string, IEnterspeedProperty>
            {
                ["Target"] = new StringEnterspeedProperty(linkField.Target),
                ["Anchor"] = new StringEnterspeedProperty(linkField.Anchor),
                ["Text"] = new StringEnterspeedProperty(linkField.Text),
                ["Title"] = new StringEnterspeedProperty(linkField.Title),
                ["Class"] = new StringEnterspeedProperty(linkField.Class),
                ["Type"] = new StringEnterspeedProperty(linkField.LinkType)
            };

            string url = null;

            if (linkField.IsInternal == false)
            {
                url = linkField.GetFriendlyUrl();
            }
            else if (linkField.IsMediaLink && linkField.TargetItem != null)
            {
                url = _urlService.GetMediaUrl(linkField.TargetItem);
            }
            else if (linkField.TargetItem != null)
            {
                url = _urlService.GetItemUrl(linkField.TargetItem);
            }

            if (string.IsNullOrEmpty(url) == false)
            {
                properties.Add("Url", new StringEnterspeedProperty(url));
            }

            return new ObjectEnterspeedProperty(field.Name, properties);
        }
    }
}