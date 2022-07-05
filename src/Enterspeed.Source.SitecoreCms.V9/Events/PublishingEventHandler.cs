using System;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.Sdk.Domain.SystemTextJson;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseItemManager _itemManager;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreIngestService _enterspeedSitecoreIngestService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreIngestService enterspeedSitecoreIngestService)
        {
            _itemManager = itemManager;
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreIngestService = enterspeedSitecoreIngestService;
        }

        public void OnItemProcessed(object sender, EventArgs args)
        {
            PublishItemContext context = args is ItemProcessedEventArgs itemProcessedEventArgs
                ? itemProcessedEventArgs.Context
                : null;

            if (context == null)
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

                EnterspeedIngestService enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new SystemTextJsonSerializer(), new EnterspeedSitecoreConfigurationProvider(_enterspeedConfigurationService));
                Language language = context.PublishOptions.Language;

                // Getting the source item first
                Item sourceItem = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.SourceDatabase);
                if (sourceItem == null)
                {
                    continue;
                }

                if (!_enterspeedSitecoreIngestService.HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was deleted or unpublished
                bool itemIsDeleted = context.Action == PublishAction.DeleteTargetItem;

                if (itemIsDeleted)
                {
                    _enterspeedSitecoreIngestService.HandleContentItem(sourceItem, enterspeedIngestService, configuration, true, false, false);
                    _enterspeedSitecoreIngestService.HandleRendering(sourceItem, enterspeedIngestService, configuration, true, false, false);
                    _enterspeedSitecoreIngestService.HandleDictionary(sourceItem, enterspeedIngestService, configuration, true, false, false);

                    continue;
                }

                // Handling if the item was published
                Item targetItem = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.TargetDatabase);
                if (targetItem == null || targetItem.Versions.Count == 0)
                {
                    continue;
                }

                if (!_enterspeedSitecoreIngestService.HasAllowedPath(targetItem))
                {
                    continue;
                }

                _enterspeedSitecoreIngestService.HandleContentItem(targetItem, enterspeedIngestService, configuration, false, true, false);
                _enterspeedSitecoreIngestService.HandleRendering(targetItem, enterspeedIngestService, configuration, false, true, false);
                _enterspeedSitecoreIngestService.HandleDictionary(targetItem, enterspeedIngestService, configuration, false, true, false);
            }
        }
    }
}