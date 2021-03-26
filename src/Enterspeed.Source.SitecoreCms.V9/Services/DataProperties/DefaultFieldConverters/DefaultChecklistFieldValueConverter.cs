using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultChecklistFieldValueConverter : IEnterspeedFieldValueConverter
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public DefaultChecklistFieldValueConverter(
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public bool CanConvert(Field field)
        {
            return field != null && field.TypeKey.Equals("checklist", StringComparison.OrdinalIgnoreCase);
        }

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            MultilistField multilistField = field;
            if (multilistField == null)
            {
                return null;
            }

            var items = multilistField.GetItems()?.ToList() ?? new List<Item>();
            if (!items.Any())
            {
                return null;
            }

            var referenceIds = items.Select(x => new StringEnterspeedProperty(null, _enterspeedIdentityService.GetId(x))).ToArray();

            return new ArrayEnterspeedProperty(field.Name, referenceIds);
        }
    }
}