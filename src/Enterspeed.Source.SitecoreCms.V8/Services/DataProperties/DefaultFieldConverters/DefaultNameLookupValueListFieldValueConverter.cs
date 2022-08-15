using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultNameLookupValueListFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly BaseItemManager _itemManager;

        public DefaultNameLookupValueListFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedIdentityService enterspeedIdentityService,
            BaseItemManager itemManager)
        {
            _fieldService = fieldService;
            _enterspeedIdentityService = enterspeedIdentityService;
            _itemManager = itemManager;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("name lookup value list", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            if (string.IsNullOrEmpty(field?.Value))
            {
                return null;
            }

            NameValueCollection values = HttpUtility.ParseQueryString(field.Value);

            if (values.Count == 0)
            {
                return null;
            }

            var items = new List<Item>();

            foreach (string key in values)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                string value = values[key];
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (!Guid.TryParse(value, out var itemId))
                {
                    continue;
                }

                Item referencedItem = _itemManager.GetItem(new ID(itemId), item.Language, Version.Latest, item.Database);
                if (referencedItem == null ||
                    referencedItem.Versions.Count == 0)
                {
                    continue;
                }

                items.Add(referencedItem);
            }

            var referenceIds = items.Select(x => new StringEnterspeedProperty(null, _enterspeedIdentityService.GetId(x.ID.ToGuid(), item.Language))).ToArray();

            return new ArrayEnterspeedProperty(_fieldService.GetFieldName(field), referenceIds);
        }
    }
}