using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultMultilistFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedSitecoreFieldService _fieldService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public DefaultMultilistFieldValueConverter(
            IEnterspeedSitecoreFieldService fieldService,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _fieldService = fieldService;
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public bool CanConvert(Field field)
        {
            return field != null &&
                (field.TypeKey.Equals("multilist", StringComparison.OrdinalIgnoreCase) ||
                    field.TypeKey.Equals("multilist with search", StringComparison.OrdinalIgnoreCase));
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters, EnterspeedSitecoreConfiguration configuration)
        {
            MultilistField multilistField = field;
            if (multilistField == null)
            {
                return null;
            }

            var itemIDs = multilistField.TargetIDs.ToList();
            if (!itemIDs.Any())
            {
                return null;
            }

            var referenceIds = itemIDs.Select(x => new StringEnterspeedProperty(null, _enterspeedIdentityService.GetId(x.ToGuid(), item.Language))).ToArray();

            return new ArrayEnterspeedProperty(_fieldService.GetFieldName(field), referenceIds);
        }
    }
}