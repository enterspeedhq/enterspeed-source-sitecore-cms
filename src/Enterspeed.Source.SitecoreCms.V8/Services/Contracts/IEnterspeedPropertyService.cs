using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedPropertyService
    {
        IDictionary<string, IEnterspeedProperty> GetProperties(Item item, EnterspeedSitecoreConfiguration configuration);

        IDictionary<string, IEnterspeedProperty> GetProperties(RenderingItem item, EnterspeedSitecoreConfiguration configuration);
    }
}