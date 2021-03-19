using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultCheckboxFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("checkbox", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            bool value = item[field.Name] == "1";

            return new BooleanEnterspeedProperty(field.Name, value);
        }
    }
}