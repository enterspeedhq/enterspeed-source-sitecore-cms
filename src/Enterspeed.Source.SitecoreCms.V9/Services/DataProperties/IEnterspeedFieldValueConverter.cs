using Enterspeed.Source.Sdk.Api.Models.Properties;
using Sitecore.Data.Fields;

namespace Enterspeed.Source.SitecoreCms.V9.Services.DataProperties
{
    public interface IEnterspeedFieldValueConverter
    {
        bool CanConvert(Field field);
        IEnterspeedProperty Convert(Field field);
    }
}