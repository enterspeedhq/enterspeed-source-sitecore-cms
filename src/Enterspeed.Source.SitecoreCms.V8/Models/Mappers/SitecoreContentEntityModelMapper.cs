using System;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Models.Mappers
{
    public class SitecoreContentEntityModelMapper : IEntityModelMapper<Item, SitecoreContentEntity>
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly IEnterspeedUrlService _urlService;

        public SitecoreContentEntityModelMapper(
            IEnterspeedIdentityService enterspeedIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            IEnterspeedUrlService urlService)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _urlService = urlService;
        }

        public SitecoreContentEntity Map(Item input, EnterspeedSitecoreConfiguration configuration)
        {
            var output = new SitecoreContentEntity
            {
                Id = _enterspeedIdentityService.GetId(input),
                Type = input.TemplateName.Replace(" ", string.Empty),
                Properties = _enterspeedPropertyService.GetProperties(input, configuration)
            };

            EnterspeedSiteInfo siteInfo = configuration.GetSite(input);
            if (siteInfo != null)
            {
                if (input.Paths.FullPath.StartsWith(siteInfo.HomeItemPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Routable content resides on or below the Home item path.
                    output.Url = _urlService.GetItemUrl(input, siteInfo);
                }

                if (!input.Paths.FullPath.Equals(siteInfo.SiteItemPath, StringComparison.OrdinalIgnoreCase))
                {
                    // If the input item is the Site item, we do not set a parent ID.
                    output.ParentId = _enterspeedIdentityService.GetId(input.Parent);
                }
            }

            return output;
        }

        public SitecoreContentEntity Map(Item input)
        {
            throw new NotImplementedException();
        }
    }
}