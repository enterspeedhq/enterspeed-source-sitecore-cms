using System;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Jobs;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class SaveEventHandler
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreJobService _enterspeedSitecoreJobService;

        public SaveEventHandler(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreJobService enterspeedSitecoreJobService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreJobService = enterspeedSitecoreJobService;
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

                if (!_enterspeedSitecoreJobService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was published
                if (sourceItem.Versions.Count == 0)
                {
                    continue;
                }

                if (!_enterspeedSitecoreJobService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                _enterspeedSitecoreJobService.HandleContentItem(sourceItem, configuration, false, true);
                _enterspeedSitecoreJobService.HandleRendering(sourceItem, configuration, false, true);
                _enterspeedSitecoreJobService.HandleDictionary(sourceItem, configuration, false, true);
            }
        }
    }
}
