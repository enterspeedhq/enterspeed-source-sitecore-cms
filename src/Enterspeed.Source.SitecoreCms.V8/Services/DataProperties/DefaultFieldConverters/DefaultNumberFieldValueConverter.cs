using System;
using System.Collections.Generic;
using System.Globalization;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultNumberFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;

        public DefaultNumberFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("number", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            if (string.IsNullOrEmpty(field.Value))
            {
                return null;
            }

            var value = 0d;

            if (field.Value.Contains("."))
            {
                double.TryParse(field.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            }
            else if (field.Value.Contains(","))
            {
                double.TryParse(field.Value, NumberStyles.Any, new CultureInfo("da-DK"), out value);
            }

            return new NumberEnterspeedProperty(_fieldService.GetFieldName(field), value);
        }
    }
}