using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedGuardService
    {
        bool CanIngest(Item item, string culture);
    }
}