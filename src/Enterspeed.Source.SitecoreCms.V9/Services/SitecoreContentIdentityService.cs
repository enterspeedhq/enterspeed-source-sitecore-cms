using System;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class SitecoreContentIdentityService : IContentIdentityService
    {
        public string GetId(Item item)
        {
            if (item == null)
            {
                return null;
            }

            return GetId(item.ID.Guid, item.Language.Name);
        }

        public string GetId(Guid itemId, string language)
        {
            return $"{itemId:N}-{language}";
        }
    }
}