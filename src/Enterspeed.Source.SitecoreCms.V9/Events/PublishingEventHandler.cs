using System;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
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
        private readonly BaseLog _log;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> _sitecoreRenderingEntityModelMapper;
        private readonly IEnterspeedIdentityService _identityService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            BaseLog log,
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> sitecoreRenderingEntityModelMapper,
            IEnterspeedIdentityService identityService,
            IEnterspeedIngestService enterspeedIngestService,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _itemManager = itemManager;
            _log = log;
            _sitecoreContentEntityModelMapper = sitecoreContentEntityModelMapper;
            _sitecoreRenderingEntityModelMapper = sitecoreRenderingEntityModelMapper;
            _identityService = identityService;
            _enterspeedIngestService = enterspeedIngestService;
            _enterspeedConfigurationService = enterspeedConfigurationService;
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

            var itemIsDeleted = context.Action == PublishAction.DeleteTargetItem;
            var itemIsPublished = context.Action == PublishAction.PublishVersion;

            if (itemIsDeleted == false && itemIsPublished == false)
            {
                return;
            }

            Language language = context.PublishOptions.Language;

            Item item = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.TargetDatabase);
            if (item == null || item.Versions.Count == 0)
            {
                return;
            }

            HandleContentItem(item, itemIsDeleted, itemIsPublished);

            HandleRenderig(item, itemIsDeleted, itemIsPublished);
        }

        private void HandleContentItem(Item item, bool itemIsDeleted, bool itemIsPublished)
        {
            // Skip, if the item published is not a content item
            if (item.Paths.FullPath.StartsWith("/sitecore/content", StringComparison.OrdinalIgnoreCase) == false)
            {
                return;
            }

            EnterspeedSitecoreConfiguration configuration = _enterspeedConfigurationService.GetConfiguration();

            EnterspeedSiteInfo siteOfItem = configuration.GetSite(item);
            if (siteOfItem == null)
            {
                // If no enabled site was found for this item, skip it
                return;
            }

            SitecoreContentEntity sitecoreContentEntity = _sitecoreContentEntityModelMapper.Map(item);
            if (sitecoreContentEntity == null)
            {
                return;
            }

            if (itemIsDeleted)
            {
                string id = _identityService.GetId(item);

                Response deleteResponse = _enterspeedIngestService.Delete(id);

                if (deleteResponse.Success == false)
                {
                    _log.Error(deleteResponse.Message ?? deleteResponse.Exception?.Message ?? "An error occurred during Enterspeed Ingest Delete API request.", deleteResponse.Exception, this);
                }
                else
                {
                    _log.Info(deleteResponse.Message, this);
                }

                return;
            }

            if (itemIsPublished)
            {
                Response saveResponse = _enterspeedIngestService.Save(sitecoreContentEntity);

                if (saveResponse.Success == false)
                {
                    _log.Error(saveResponse.Message ?? saveResponse.Exception?.Message ?? "An error occurred during Enterspeed Ingest Save API request.", saveResponse.Exception, this);
                }
                else
                {
                    _log.Info(saveResponse.Message, this);
                }
            }
        }

        private void HandleRenderig(Item item, bool itemIsDeleted, bool itemIsPublished)
        {
            // Skip, if the item published is not a rendering item
            if (item.Paths.FullPath.StartsWith("/sitecore/layout/Renderings", StringComparison.OrdinalIgnoreCase) == false)
            {
                return;
            }

            RenderingItem renderingItem = item;
            if (renderingItem?.InnerItem == null)
            {
                return;
            }

            SitecoreRenderingEntity sitecoreRenderingEntity = _sitecoreRenderingEntityModelMapper.Map(renderingItem);

            if (itemIsDeleted)
            {
                string id = _identityService.GetId(renderingItem);

                Response deleteResponse = _enterspeedIngestService.Delete(id);

                if (deleteResponse.Success == false)
                {
                    _log.Error(deleteResponse.Message ?? deleteResponse.Exception?.Message ?? "An error occurred during Enterspeed Ingest Delete API request.", deleteResponse.Exception, this);
                }
                else
                {
                    _log.Info(deleteResponse.Message, this);
                }

                return;
            }

            if (itemIsPublished)
            {
                Response saveResponse = _enterspeedIngestService.Save(sitecoreRenderingEntity);

                if (saveResponse.Success == false)
                {
                    _log.Error(saveResponse.Message ?? saveResponse.Exception?.Message ?? "An error occurred during Enterspeed Ingest Save API request.", saveResponse.Exception, this);
                }
                else
                {
                    _log.Info(saveResponse.Message, this);
                }
            }
        }
    }
}