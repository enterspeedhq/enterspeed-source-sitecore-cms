using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedGuardService : IEnterspeedGuardService
    {
        public EnterspeedGuardService()
        {

        }

        //TODO: Implement guards
        public bool CanIngest(Item item, string culture)
        {
            return true;
        }
    }
}