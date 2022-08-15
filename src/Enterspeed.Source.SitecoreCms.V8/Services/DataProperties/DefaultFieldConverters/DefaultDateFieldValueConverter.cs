using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.Formatters;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultDateFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly EnterspeedDateFormatter _dateFormatter;

        public DefaultDateFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            EnterspeedDateFormatter dateFormatter)
        {
            _fieldService = fieldService;
            _dateFormatter = dateFormatter;
        }

        public bool CanConvert(Field field)
        {
            return field != null &&
                (field.TypeKey.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                    field.TypeKey.Equals("datetime", StringComparison.OrdinalIgnoreCase));
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            DateField dateField = field;
            if (dateField == null ||
                dateField.DateTime == DateTime.MinValue)
            {
                return null;
            }

            return new StringEnterspeedProperty(_fieldService.GetFieldName(field), _dateFormatter.FormatDate(dateField.DateTime));
        }
    }
}