using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultNameValueListFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("name value list", StringComparison.OrdinalIgnoreCase);
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

            var list = new List<StringEnterspeedProperty>();

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

                list.Add(new StringEnterspeedProperty(key, value));
            }

            return new ArrayEnterspeedProperty(field.Name, list.ToArray());
        }
    }
}