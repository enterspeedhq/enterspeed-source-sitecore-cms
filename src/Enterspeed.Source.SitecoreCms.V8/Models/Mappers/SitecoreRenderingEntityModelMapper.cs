using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Models.Mappers
{
    public class SitecoreRenderingEntityModelMapper : IEntityModelMapper<RenderingItem, SitecoreRenderingEntity>
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;

        public SitecoreRenderingEntityModelMapper(
            IEnterspeedIdentityService enterspeedIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
        }

        public SitecoreRenderingEntity Map(RenderingItem input, EnterspeedSitecoreConfiguration configuration)
        {
            return new SitecoreRenderingEntity
            {
                Id = _enterspeedIdentityService.GetId(input),
                Type = "rendering",
                Properties = _enterspeedPropertyService.GetProperties(input, configuration)
            };
        }
    }
}