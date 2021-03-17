using Enterspeed.Source.Sdk.Api.Models.Properties;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties
{
    public interface IEnterspeedDataSourceConverter
    {
        bool CanConvert(Item dataSourceItem);
        IEnterspeedProperty Convert(Item dataSourceItem, string culture);
    }
}