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
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public DefaultGeneralLinkFieldValueConverter(
            IEnterspeedUrlService urlService,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _urlService = urlService;
            _enterspeedIdentityService = enterspeedIdentityService;
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

            var properties = new Dictionary<string, IEnterspeedProperty>();

            if (string.IsNullOrEmpty(linkField.Target) == false)
            {
                properties.Add("Target", new StringEnterspeedProperty(linkField.Target));
            }

            if (string.IsNullOrEmpty(linkField.Anchor) == false)
            {
                properties.Add("Anchor", new StringEnterspeedProperty(linkField.Anchor));
            }

            if (string.IsNullOrEmpty(linkField.Text) == false)
            {
                properties.Add("Text", new StringEnterspeedProperty(linkField.Text));
            }

            if (string.IsNullOrEmpty(linkField.Title) == false)
            {
                properties.Add("Title", new StringEnterspeedProperty(linkField.Title));
            }

            if (string.IsNullOrEmpty(linkField.Class) == false)
            {
                properties.Add("Class", new StringEnterspeedProperty(linkField.Class));
            }

            string url = null;

            if (linkField.LinkType == "media")
            {
                url = _urlService.GetMediaUrl(linkField.TargetItem);
            }
            else if (linkField.LinkType == "internal")
            {
                url = _urlService.GetItemUrl(linkField.TargetItem);

                if (linkField.TargetItem != null)
                {
                    properties.Add("TargetId", new StringEnterspeedProperty(_enterspeedIdentityService.GetId(linkField.TargetItem)));
                }
            }
            else if (linkField.LinkType == "external" ||
                linkField.LinkType == "anchor" ||
                linkField.LinkType == "mailto" ||
                linkField.LinkType == "javascript")
            {
                url = linkField.GetFriendlyUrl();
            }

            if (string.IsNullOrEmpty(url) == false)
            {
                properties.Add("Url", new StringEnterspeedProperty(url));
            }
            else
            {
                return null;
            }

            return new ObjectEnterspeedProperty(field.Name, properties);
        }
    }
}