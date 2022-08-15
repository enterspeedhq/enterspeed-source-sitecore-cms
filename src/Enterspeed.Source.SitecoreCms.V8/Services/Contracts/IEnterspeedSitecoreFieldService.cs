using Sitecore.Data.Fields;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedSitecoreFieldService
    {
        string GetFieldName(Field field);
    }
}