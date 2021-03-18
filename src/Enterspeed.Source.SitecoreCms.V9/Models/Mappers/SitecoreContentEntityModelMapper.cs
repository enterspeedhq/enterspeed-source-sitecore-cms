using System;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
{
    public class SitecoreContentEntityModelMapper
    {
        private readonly IContentIdentityService _contentIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedUrlService _urlService;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public SitecoreContentEntityModelMapper(
            IContentIdentityService contentIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedUrlService urlService,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _contentIdentityService = contentIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _urlService = urlService;
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public SitecoreContentEntity Map(Item item)
        {
            var output = new SitecoreContentEntity
            {
                Id = _contentIdentityService.GetId(item),
                Type = item.TemplateName,
                Url = _urlService.GetItemUrl(item),
                Properties = _enterspeedPropertyService.GetProperties(item)
            };

            EnterspeedSiteInfo siteInfo = _enterspeedConfigurationService.GetConfiguration().GetSite(item);
            if (siteInfo != null &&
                item.Paths.FullPath.Equals(siteInfo.HomeItemPath, StringComparison.OrdinalIgnoreCase) == false)
            {
                output.ParentId = _contentIdentityService.GetId(item.Parent);
            }

            return output;
        }
    }
}