using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultDroplistFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;

        public DefaultDroplistFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("droplist", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            ReferenceField referenceField = field;
            if (string.IsNullOrEmpty(referenceField?.Value))
            {
                return null;
            }

            return new StringEnterspeedProperty(_fieldService.GetFieldName(field), referenceField.Value);
        }
    }
}