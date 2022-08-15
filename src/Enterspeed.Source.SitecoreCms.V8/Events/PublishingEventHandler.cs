using System;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseItemManager _itemManager;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreJobService _enterspeedSitecoreJobService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreJobService enterspeedSitecoreJobService)
        {
            _itemManager = itemManager;
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _enterspeedSitecoreJobService = enterspeedSitecoreJobService;
        }

        public void OnItemProcessed(object sender, EventArgs args)
        {
            var context = args is ItemProcessedEventArgs itemProcessedEventArgs
                ? itemProcessedEventArgs.Context
                : null;

            if (context == null)
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

                var language = context.PublishOptions.Language;

                // Getting the source item first
                var sourceItem = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.SourceDatabase);
                if (sourceItem == null)
                {
                    continue;
                }

                if (!HasAllowedPath(sourceItem))
                {
                    continue;
                }

                // Handling if the item was deleted or unpublished
                var itemIsDeleted = context.Action == PublishAction.DeleteTargetItem;
                if (!itemIsDeleted)
                {
                    _enterspeedSitecoreJobService.HandleContentItem(sourceItem, configuration, false, true);
                    _enterspeedSitecoreJobService.HandleRendering(sourceItem, configuration, false, true);
                    _enterspeedSitecoreJobService.HandleDictionary(sourceItem, configuration, false, true);
                }
            }
        }

        private static bool HasAllowedPath(Item item)
        {
            return item.IsContentItem() || item.IsRenderingItem() || item.IsDictionaryItem();
        }
    }
}
