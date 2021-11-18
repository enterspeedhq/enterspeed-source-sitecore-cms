using System;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public class EnterspeedSiteInfo
    {
        public string Name { get; set; }

        public string BaseUrl { get; set; }

        public string MediaBaseUrl { get; set; }

        public string PublishHookUrl { get; set; }

        public string HomeItemPath { get; set; }

        public string SiteItemPath { get; set; }

        public bool IsHttpsEnabled { get; set; }

        public string Language { get; set; }

        public bool IsItemOfSite(Item item)
        {
            if (string.IsNullOrEmpty(SiteItemPath))
            {
                return false;
            }

            if (item == null)
            {
                return false;
            }

            return item.Paths.FullPath.StartsWith(SiteItemPath, StringComparison.OrdinalIgnoreCase) && item.Language.Name == Language;
        }
    }
}