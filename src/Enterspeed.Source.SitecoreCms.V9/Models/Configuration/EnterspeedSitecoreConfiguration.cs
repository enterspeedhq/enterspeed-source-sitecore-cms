using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Configuration;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public class EnterspeedSitecoreConfiguration : EnterspeedConfiguration
    {
        public bool IsEnabled { get; set; }

        public string ItemNotFoundUrl { get; set; }

        public List<EnterspeedSiteInfo> SiteInfos { get; } = new List<EnterspeedSiteInfo>();

        public EnterspeedSiteInfo GetSite(Item item)
        {
            if (item == null)
            {
                return null;
            }

            return SiteInfos?.FirstOrDefault(x => x.IsItemOfSite(item));
        }
    }
}