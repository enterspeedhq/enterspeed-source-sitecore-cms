using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultChecklistFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("checklist", StringComparison.OrdinalIgnoreCase);
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
                var properties = MapNestedProperties(itemInList, siteInfo, fieldValueConverters);
                list.Add(new ObjectEnterspeedProperty(itemInList.Name, properties));
            }

            return new ArrayEnterspeedProperty(field.Name, list.ToArray());
        }

        private static IDictionary<string, IEnterspeedProperty> MapNestedProperties(Item item, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            var output = new Dictionary<string, IEnterspeedProperty>();

            FieldCollection fieldsCollection = item.Fields;
            if (fieldsCollection == null || fieldsCollection.Any() == false)
            {
                return output;
            }

            // Exclude system fields
            List<Field> fields = fieldsCollection.Where(field =>
                    field.InnerItem.Paths.FullPath.StartsWith("/sitecore/templates/system", StringComparison.OrdinalIgnoreCase) == false)
                .ToList();

            if (fields.Any() == false)
            {
                return output;
            }

            foreach (Field field in fields)
            {
                if (field == null)
                {
                    continue;
                }

                IEnterspeedFieldValueConverter converter = fieldValueConverters.FirstOrDefault(x => x.CanConvert(field));

                var value = converter?.Convert(item, field, siteInfo, fieldValueConverters);
                if (value == null)
                {
                    continue;
                }

                output.Add(field.Name, value);
            }

            return output;
        }
    }
}