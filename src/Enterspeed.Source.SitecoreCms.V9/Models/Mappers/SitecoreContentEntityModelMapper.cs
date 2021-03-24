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

        public SitecoreContentEntity Map(Item input)
        {
            var output = new SitecoreContentEntity
            {
                Id = _enterspeedIdentityService.GetId(input),
                Type = input.TemplateName,
                Url = _urlService.GetItemUrl(input),
                Properties = _enterspeedPropertyService.GetProperties(input)
            };

            EnterspeedSiteInfo siteInfo = _enterspeedConfigurationService.GetConfigurationFromSitecore().GetSite(input);
            if (siteInfo != null &&
                input.Paths.FullPath.Equals(siteInfo.SiteItemPath, StringComparison.OrdinalIgnoreCase) == false)
            {
                // If the input item is the Site item, we do not set a parent ID.
                output.ParentId = _enterspeedIdentityService.GetId(input.Parent);
            }

            return output;
        }
    }
}