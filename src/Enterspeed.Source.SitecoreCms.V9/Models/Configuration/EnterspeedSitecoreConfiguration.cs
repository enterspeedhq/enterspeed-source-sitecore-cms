using System.Collections.Generic;
using Enterspeed.Source.Sdk.Configuration;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public class EnterspeedSitecoreConfiguration : EnterspeedConfiguration
    {
        public string ItemNotFoundUrl { get; set; }
        public bool IsHttpsEnabled { get; set; }
        public List<EnterspeedSiteInfo> SiteInfo { get; } = new List<EnterspeedSiteInfo>();
    }
}