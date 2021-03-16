using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Mappers
{
    public class SitecoreContentEntityModelMapper
    {
        private readonly IContentIdentityService _contentIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;

        public SitecoreContentEntityModelMapper(
            IContentIdentityService contentIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService)
        {
            _contentIdentityService = contentIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
        }

        public SitecoreContentEntity Map(Item item, string culture)
        {
            var output = new SitecoreContentEntity();

            output.Id = _contentIdentityService.GetId(item, culture);

            output.Properties = _enterspeedPropertyService.GetProperties(item, culture);

            return output;
        }
    }
}