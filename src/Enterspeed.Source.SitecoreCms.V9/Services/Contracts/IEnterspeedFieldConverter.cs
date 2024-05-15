﻿using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.Contracts
{
    public interface IEnterspeedFieldConverter
    {
        IDictionary<string, IEnterspeedProperty> ConvertFields(Item item, EnterspeedSiteInfo siteInfo, List<IEnterspeedFieldValueConverter> fieldValueConverters,  EnterspeedSitecoreConfiguration configuration);
    }
}