using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultMultilistFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedFieldConverter _fieldConverter;

        public DefaultMultilistFieldValueConverter(
            IEnterspeedFieldConverter fieldConverter)
        {
            _fieldConverter = fieldConverter;
        }

        public bool CanConvert(Field field)
        {
            return field != null &&
                (field.TypeKey.Equals("multilist", StringComparison.OrdinalIgnoreCase) ||
                    field.TypeKey.Equals("multilist with search", StringComparison.OrdinalIgnoreCase));
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            MultilistField multilistField = field;
            if (multilistField == null)
            {
                return null;
            }

            var items = multilistField.GetItems()?.ToList() ?? new List<Item>();
            if (items.Any() == false)
            {
                return null;
            }

            var list = new List<ObjectEnterspeedProperty>();

            foreach (var itemInList in items)
            {
                var properties = _fieldConverter.ConvertFields(itemInList, siteInfo, fieldValueConverters);
                list.Add(new ObjectEnterspeedProperty(null, properties));
            }

            return new ArrayEnterspeedProperty(field.Name, list.ToArray());
        }
    }
}