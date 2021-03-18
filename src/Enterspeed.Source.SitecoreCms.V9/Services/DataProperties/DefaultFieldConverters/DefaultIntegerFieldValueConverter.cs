using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters
{
    public class DefaultIntegerFieldValueConverter : IEnterspeedFieldValueConverter
    {
        public bool CanConvert(Field field) => throw new System.NotImplementedException();

        public IEnterspeedProperty Convert(Item item, Field field, EnterspeedSiteInfo siteInfo) => throw new System.NotImplementedException();
    }
}