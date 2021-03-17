using System;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public class EnterspeedSiteInfo
    {
        public string Name { get; set; }

        public string BaseUrl { get; set; }

        public string StartPath { get; set; }

        public bool IsItemOfSite(Item item)
        {
            if (string.IsNullOrEmpty(StartPath))
            {
                return false;
            }

            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith(StartPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}