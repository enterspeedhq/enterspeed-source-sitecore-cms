using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedFieldConverter : IEnterspeedFieldConverter
    {
        private readonly List<IEnterspeedFieldValueConverter> _fieldValueConverters;

        public EnterspeedFieldConverter(
            List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            _fieldValueConverters = fieldValueConverters;
        }

        public IDictionary<string, IEnterspeedProperty> ConvertFields(Item item, EnterspeedSiteInfo siteInfo)
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

                IEnterspeedFieldValueConverter converter = _fieldValueConverters.FirstOrDefault(x => x.CanConvert(field));

                var value = converter?.Convert(item, field, siteInfo, _fieldValueConverters);
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