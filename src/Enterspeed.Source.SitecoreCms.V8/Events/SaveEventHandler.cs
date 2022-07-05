using System;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Providers;
using Enterspeed.Source.SitecoreCms.V8.Services;
using Enterspeed.Source.SitecoreCms.V8.Services.Serializers;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class SaveEventHandler
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreIngestService _enterspeedSitecoreIngestService;

        public SaveEventHandler(
            BaseItemManager itemManager,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreIngestService enterspeedSitecoreIngestService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreIngestService = enterspeedSitecoreIngestService;
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            SitecoreEventArgs eventArgs = args as SitecoreEventArgs;

            Item sourceItem = eventArgs.Parameters[0] as Item;

            if (sourceItem == null)
            {
                return;
            }

            var siteConfigurations = _enterspeedConfigurationService.GetConfiguration();
            foreach (EnterspeedSitecoreConfiguration configuration in siteConfigurations)
            {
                if (!configuration.IsEnabled)
                {
                    continue;
                }

                if (!configuration.IsPreview)
                {
                    continue;
                }

                EnterspeedIngestService enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new NewtonsoftJsonSerializer(), new EnterspeedSitecoreConfigurationProvider(_enterspeedConfigurationService));

                // Getting the source item first
                if (sourceItem == null)
                {
                    continue;
                }

                if (!_enterspeedSitecoreIngestService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was published
                if (sourceItem == null || sourceItem.Versions.Count == 0)
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