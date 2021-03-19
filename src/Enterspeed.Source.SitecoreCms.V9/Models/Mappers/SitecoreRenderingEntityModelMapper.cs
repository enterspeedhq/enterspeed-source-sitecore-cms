using System;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;
using Sitecore.Mvc.Names;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
{
    public class SitecoreRenderingEntityModelMapper : IEntityModelMapper<RenderingItem, SitecoreRenderingEntity>
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;

        public SitecoreRenderingEntityModelMapper(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedIdentityService enterspeedIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedIdentityService = enterspeedIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
        }

        public SitecoreRenderingEntity Map(RenderingItem renderingItem)
        {
            EnterspeedSitecoreConfiguration configuration = _enterspeedConfigurationService.GetConfiguration();

            var output = new SitecoreRenderingEntity();

            output.Id = _enterspeedIdentityService.GetId(renderingItem);
            output.Type = "rendering";
            output.Properties = _enterspeedPropertyService.GetProperties(renderingItem);

            if (renderingItem.InnerItem.TemplateID == TemplateIds.ControllerRendering)
            {
                var controller = renderingItem.InnerItem["Controller"];
                var controllerAction = renderingItem.InnerItem["Controller Action"];

                if (string.IsNullOrEmpty(controller) == false && string.IsNullOrEmpty(controllerAction) == false)
                {
                    var controllerSplit = controller.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (controllerSplit.Length == 2)
                    {
                        output.Url = $"{configuration.RenderingsBaseUrl.TrimEnd('/')}/{controllerSplit[0].Replace("Controller", string.Empty)}/{controllerAction}".ToLower();
                    }
                }
            }

            return output;
        }
    }
}