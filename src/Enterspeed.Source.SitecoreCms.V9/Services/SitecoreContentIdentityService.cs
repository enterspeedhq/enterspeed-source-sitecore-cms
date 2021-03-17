using System;
using Sitecore.Data.Items;
using Sitecore.Globalization;

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

            return GetId(item.ID.Guid, item.Language);
        }

        public string GetId(Guid itemId, Language language)
        {
            return $"{itemId:N}-{language.Name}";
        }
    }
}