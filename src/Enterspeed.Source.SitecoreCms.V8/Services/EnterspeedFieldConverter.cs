using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V8.Services.DataProperties;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedFieldConverter : IEnterspeedFieldConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;

        public EnterspeedFieldConverter(
            IEnterspeedSitecoreFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public IDictionary<string, IEnterspeedProperty> ConvertFields(Item item, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            var output = new Dictionary<string, IEnterspeedProperty>();

            FieldCollection fieldsCollection = item.Fields;
            if (fieldsCollection == null || !fieldsCollection.Any())
            {
                return output;
            }

            // Mapping all fields, by default
            List<Field> fields = fieldsCollection.ToList();

            if (item.IsDictionaryItem())
            {
                // Only include dictionary fields
                fields = fields.Where(field =>
                        field.InnerItem != null &&
                        field.InnerItem.Paths.FullPath.StartsWith("/sitecore/templates/System/Dictionary", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else if (item.IsContentItem() || item.IsRenderingItem())
            {
                // Exclude system fields for content and renderings
                fields = fields.Where(field =>
                        field.InnerItem != null &&
                        !field.InnerItem.Paths.FullPath.StartsWith("/sitecore/templates/system", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!fields.Any())
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

                var value = converter?.Convert(item, field, siteInfo, fieldValueConverters, configuration);
                if (value == null)
                {
                    continue;
                }

                output.Add(_fieldService.GetFieldName(field), value);
            }

            return output;
        }
    }
}