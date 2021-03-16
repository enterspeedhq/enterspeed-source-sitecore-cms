using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public interface IEnterspeedPropertyService
    {
        IDictionary<string, IEnterspeedProperty> GetProperties(Item item, string culture);
    }
}