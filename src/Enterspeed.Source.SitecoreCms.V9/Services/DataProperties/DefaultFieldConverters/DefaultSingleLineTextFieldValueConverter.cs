using System;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Sitecore.Data.Fields;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultSingleLineTextFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("single-line text", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Field field)
        {
            return new StringEnterspeedProperty(field.Name, field.Value);
        }
    }
}