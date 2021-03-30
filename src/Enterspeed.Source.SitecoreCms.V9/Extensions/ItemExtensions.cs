using System;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsContentItem(this Item item)
        {
            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith("/sitecore/content", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRenderingItem(this Item item)
        {
            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith("/sitecore/layout/renderings", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDictionaryItem(this Item item)
        {
            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith("/sitecore/system/dictionary", StringComparison.OrdinalIgnoreCase);
        }
    }
}