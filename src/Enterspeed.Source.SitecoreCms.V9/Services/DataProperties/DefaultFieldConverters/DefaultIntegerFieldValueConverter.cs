using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultIntegerFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("integer", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            var value = 0;

            if (string.IsNullOrEmpty(field.Value) == false)
            {
                value = int.Parse(field.Value);
            }

            return new NumberEnterspeedProperty(field.Name, value);
        }
    }
}