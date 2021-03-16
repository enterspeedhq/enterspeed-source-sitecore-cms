using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class SitecoreContentIdentityService : IContentIdentityService
    {
        public string GetId(Item item, string culture)
        {
            if (item == null)
            {
                return null;
            }

            return $"{item.ID.Guid:N}-{culture}";
        }
    }
}