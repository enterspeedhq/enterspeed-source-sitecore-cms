using System;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
{
    public class SitecoreContentEntityModelMapper : IEntityModelMapper<Item, SitecoreContentEntity>
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedUrlService _urlService;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public SitecoreContentEntityModelMapper(
            IEnterspeedIdentityService enterspeedIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedUrlService urlService,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _urlService = urlService;
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public SitecoreContentEntity Map(Item renderingItem)
        {
            var output = new SitecoreContentEntity
            {
                Id = _enterspeedIdentityService.GetId(renderingItem),
                Type = renderingItem.TemplateName,
                Url = _urlService.GetItemUrl(renderingItem),
                Properties = _enterspeedPropertyService.GetProperties(renderingItem)
            };

            EnterspeedSiteInfo siteInfo = _enterspeedConfigurationService.GetConfiguration().GetSite(renderingItem);
            if (siteInfo != null &&
                renderingItem.Paths.FullPath.Equals(siteInfo.HomeItemPath, StringComparison.OrdinalIgnoreCase) == false)
            {
                output.ParentId = _enterspeedIdentityService.GetId(renderingItem.Parent);
            }

            return output;
        }
    }
}