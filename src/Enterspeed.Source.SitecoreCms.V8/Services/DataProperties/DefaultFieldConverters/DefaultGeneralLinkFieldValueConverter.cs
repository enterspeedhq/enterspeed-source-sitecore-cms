using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultGeneralLinkFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private const string PropertyTarget = "target";
        private const string PropertyAnchor = "anchor";
        private const string PropertyText = "text";
        private const string PropertyTitle = "title";
        private const string PropertyClass = "class";
        private const string PropertyTargetId = "targetid";
        private const string PropertyTargetType = "targettype";
        private const string PropertyUrl = "url";

        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedUrlService _urlService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public DefaultGeneralLinkFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedUrlService urlService,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _fieldService = fieldService;
            _urlService = urlService;
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public bool CanConvert(Field field)
        {
            return field != null &&
                (field.TypeKey.Equals("general link", StringComparison.OrdinalIgnoreCase) ||
                    field.TypeKey.Equals("general link with search", StringComparison.OrdinalIgnoreCase));
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            LinkField linkField = field;
            if (linkField == null)
            {
                return null;
            }

            var properties = new Dictionary<string, IEnterspeedProperty>();

            if (!string.IsNullOrEmpty(linkField.Target))
            {
                properties.Add(PropertyTarget, new StringEnterspeedProperty(PropertyTarget, linkField.Target));
            }

            if (!string.IsNullOrEmpty(linkField.Anchor))
            {
                properties.Add(PropertyAnchor, new StringEnterspeedProperty(PropertyAnchor, linkField.Anchor));
            }

            if (!string.IsNullOrEmpty(linkField.Text))
            {
                properties.Add(PropertyText, new StringEnterspeedProperty(PropertyText, linkField.Text));
            }

            if (!string.IsNullOrEmpty(linkField.Title))
            {
                properties.Add(PropertyTitle, new StringEnterspeedProperty(PropertyTitle, linkField.Title));
            }

            if (!string.IsNullOrEmpty(linkField.Class))
            {
                properties.Add(PropertyClass, new StringEnterspeedProperty(PropertyClass, linkField.Class));
            }

            string url = null;

            if (linkField.LinkType == "media")
            {
                url = _urlService.GetMediaUrl(linkField.TargetItem, siteInfo);
            }
            else if (linkField.LinkType == "internal")
            {
                url = _urlService.GetItemUrl(linkField.TargetItem, siteInfo);

                if (linkField.TargetItem != null)
                {
                    properties.Add(PropertyTargetType, new StringEnterspeedProperty(PropertyTargetType, linkField.TargetItem.TemplateName));
                    properties.Add(PropertyTargetId, new StringEnterspeedProperty(PropertyTargetId, _enterspeedIdentityService.GetId(linkField.TargetID.ToGuid(), item.Language)));
                }
            }
            else if (linkField.LinkType == "external" ||
                linkField.LinkType == "anchor" ||
                linkField.LinkType == "mailto" ||
                linkField.LinkType == "javascript")
            {
                url = linkField.GetFriendlyUrl();
            }

            if (!string.IsNullOrEmpty(url))
            {
                properties.Add(PropertyUrl, new StringEnterspeedProperty(PropertyUrl, url));
            }
            else
            {
                return null;
            }

            return new ObjectEnterspeedProperty(_fieldService.GetFieldName(field), properties);
        }
    }
}