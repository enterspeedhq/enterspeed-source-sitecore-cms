using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultIntegerFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;

        public DefaultIntegerFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("integer", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            var value = 0;

            if (!string.IsNullOrEmpty(field.Value))
            {
                value = int.Parse(field.Value);
            }

            return new NumberEnterspeedProperty(_fieldService.GetFieldName(field), value);
        }
    }
}