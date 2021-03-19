using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultNameLookupValueListFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedFieldConverter _fieldConverter;
        private readonly BaseItemManager _itemManager;

        public DefaultNameLookupValueListFieldValueConverter(
            IEnterspeedFieldConverter fieldConverter,
            BaseItemManager itemManager)
        {
            _fieldConverter = fieldConverter;
            _itemManager = itemManager;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("name lookup value list", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
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

            var list = new List<ObjectEnterspeedProperty>();

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

                if (Guid.TryParse(value, out var itemId) == false)
                {
                    continue;
                }

                Item referencedItem = _itemManager.GetItem(new ID(itemId), item.Language, Version.Latest, item.Database);
                if (referencedItem == null ||
                    referencedItem.Versions.Count == 0)
                {
                    continue;
                }

                IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(referencedItem, siteInfo, fieldValueConverters);

                list.Add(new ObjectEnterspeedProperty(field.Name, properties));
            }

            return new ArrayEnterspeedProperty(field.Name, list.ToArray());
        }
    }
}