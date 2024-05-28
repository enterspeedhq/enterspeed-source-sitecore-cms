using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
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
            var output = new SitecoreRenderingEntity();

            output.Id = _enterspeedIdentityService.GetId(input);
            output.Type = "rendering";
            output.Properties = _enterspeedPropertyService.GetProperties(input, configuration);

            return output;
        }
    }
}