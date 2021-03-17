using System;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Links.UrlBuilders;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
{
    public class SitecoreContentEntityModelMapper
    {
        private readonly IContentIdentityService _contentIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly BaseLinkManager _linkManager;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public SitecoreContentEntityModelMapper(
            IContentIdentityService contentIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            BaseLinkManager linkManager,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _contentIdentityService = contentIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _linkManager = linkManager;
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public SitecoreContentEntity Map(Item item)
        {
            var output = new SitecoreContentEntity();

            output.Id = _contentIdentityService.GetId(item);
            output.Type = item.TemplateName;
            output.ParentId = _contentIdentityService.GetId(item.Parent);

            string itemUrl = _linkManager.GetItemUrl(item, new ItemUrlBuilderOptions
            {
                SiteResolving = true,
                AlwaysIncludeServerUrl = true
            });

            if (_enterspeedConfigurationService.GetConfiguration().IsHttpsEnabled &&
                itemUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == false)
            {
                itemUrl = itemUrl.Replace("http://", "https://");
            }

            output.Url = itemUrl;

            output.Properties = _enterspeedPropertyService.GetProperties(item);

            return output;
        }
    }
}