using System;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class SaveEventHandler
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreJobSeederService _enterspeedSitecoreJobSeederService;

        public SaveEventHandler(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreJobSeederService enterspeedSitecoreJobSeederService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreJobSeederService = enterspeedSitecoreJobSeederService;
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            var eventArgs = args as SitecoreEventArgs;

            if (!(eventArgs.Parameters[0] is Item sourceItem))
            {
                return;
            }

            var siteConfigurations = _enterspeedConfigurationService.GetConfigurations();
            foreach (var configuration in siteConfigurations)
            {
                if (!configuration.IsEnabled)
                {
                    continue;
                }

                if (!configuration.IsPreview)
                {
                    continue;
                }

                if (!_enterspeedSitecoreJobSeederService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was published
                if (sourceItem.Versions.Count == 0)
                {
                    continue;
                }

                if (!_enterspeedSitecoreJobSeederService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                _enterspeedSitecoreJobSeederService.HandleContentItem(sourceItem, configuration, false, true);
                _enterspeedSitecoreJobSeederService.HandleRendering(sourceItem, configuration, false, true);
                _enterspeedSitecoreJobSeederService.HandleDictionary(sourceItem, configuration, false, true);
            }
        }
    }
}