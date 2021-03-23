using System;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public class EnterspeedSiteInfo
    {
        public string Name { get; set; }

        public string BaseUrl { get; set; }

        public string SiteItemPath { get; set; }

        public string HomeItemPath { get; set; }

        public bool IsHttpsEnabled { get; set; }

        public bool IsItemOfSite(Item item)
        {
            if (string.IsNullOrEmpty(HomeItemPath))
            {
                return false;
            }

            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith(HomeItemPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}