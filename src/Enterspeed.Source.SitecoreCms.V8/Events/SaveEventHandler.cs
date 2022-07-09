using System;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V8.Providers;
using Enterspeed.Source.SitecoreCms.V8.Services;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V8.Services.Serializers;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class SaveEventHandler
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreIngestService _enterspeedSitecoreIngestService;

        public SaveEventHandler(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreIngestService enterspeedSitecoreIngestService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreIngestService = enterspeedSitecoreIngestService;
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

                var enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new NewtonsoftJsonSerializer(), new EnterspeedSitecoreConfigurationProvider(_enterspeedConfigurationService));

                if (!_enterspeedSitecoreIngestService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was published
                if (sourceItem.Versions.Count == 0)
                {
                    continue;
                }

                if (!_enterspeedSitecoreIngestService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                _enterspeedSitecoreIngestService.HandleContentItem(sourceItem, enterspeedIngestService, configuration, false, true, true);
                _enterspeedSitecoreIngestService.HandleRendering(sourceItem, enterspeedIngestService, configuration, false, true, true);
                _enterspeedSitecoreIngestService.HandleDictionary(sourceItem, enterspeedIngestService, configuration, false, true, true);
            }
        }
    }
}