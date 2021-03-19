using System;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedSitecoreIdentityService : IEnterspeedIdentityService
    {
        public string GetId(Item item)
        {
            if (item == null)
            {
                return null;
            }

            return GetId(item.ID.Guid, item.Language);
        }

        public string GetId(RenderingItem renderingItem)
        {
            if (renderingItem == null)
            {
                return null;
            }

            return $"{renderingItem.ID.Guid:N}";
        }

        public string GetId(Guid itemId, Language language)
        {
            if (language == null)
            {
                return $"{itemId:N}";
            }

            return $"{itemId:N}-{language.Name}";
        }
    }
}