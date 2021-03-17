using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Links.UrlBuilders;

namespace Enterspeed.Source.SitecoreCms.V9.Mappers
{
    public class SitecoreContentEntityModelMapper
    {
        private readonly IContentIdentityService _contentIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;
        private readonly BaseLinkManager _linkManager;

        public SitecoreContentEntityModelMapper(
            IContentIdentityService contentIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService,
            BaseLinkManager linkManager)
        {
            _contentIdentityService = contentIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
            _linkManager = linkManager;
        }

        public SitecoreContentEntity Map(Item item)
        {
            var output = new SitecoreContentEntity();

            output.Id = _contentIdentityService.GetId(item);
            output.Type = item.TemplateName;
            output.ParentId = _contentIdentityService.GetId(item.Parent);
            output.Url = _linkManager.GetItemUrl(item, new ItemUrlBuilderOptions
            {
                AlwaysIncludeServerUrl = true
            });

            output.Properties = _enterspeedPropertyService.GetProperties(item);

            return output;
        }
    }
}