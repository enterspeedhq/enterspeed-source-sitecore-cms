using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultDroptreeFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public DefaultDroptreeFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _fieldService = fieldService;
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("droptree", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            ReferenceField referenceField = field;
            if (referenceField?.TargetItem == null)
            {
                return null;
            }

            return new StringEnterspeedProperty(_fieldService.GetFieldName(field), _enterspeedIdentityService.GetId(referenceField.TargetID.ToGuid(), item.Language));
        }
    }
}