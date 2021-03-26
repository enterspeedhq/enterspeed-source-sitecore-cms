using System;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseItemManager _itemManager;
        private readonly BaseLinkStrategyFactory _linkStrategyFactory;
        private readonly BaseLog _log;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> _sitecoreRenderingEntityModelMapper;
        private readonly IEnterspeedIdentityService _identityService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            BaseLinkStrategyFactory linkStrategyFactory,
            BaseLog log,
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> sitecoreRenderingEntityModelMapper,
            IEnterspeedIdentityService identityService,
            IEnterspeedIngestService enterspeedIngestService,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _itemManager = itemManager;
            _linkStrategyFactory = linkStrategyFactory;
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

            Language language = context.PublishOptions.Language;

            // Getting the source item first
            Item sourceItem = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.SourceDatabase);
            if (sourceItem == null)
            {
                return;
            }

            if (!HasAllowedPath(sourceItem))
            {
                return;
            }

            // Handling if the item was deleted or unpublished
            bool itemIsDeleted = context.Action == PublishAction.DeleteTargetItem;

            if (itemIsDeleted)
            {
                HandleContentItem(sourceItem, true, false);
                HandleRendering(sourceItem, true, false);

                return;
            }

            // Handling if the item was published
            Item targetItem = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.TargetDatabase);
            if (targetItem == null || targetItem.Versions.Count == 0)
            {
                return;
            }

            if (!HasAllowedPath(targetItem))
            {
                return;
            }

            HandleContentItem(targetItem, false, true);
            HandleRendering(targetItem, false, true);
        }

        private static bool HasAllowedPath(Item item)
        {
            return HasContentPath(item) || HasRenderingsPath(item);
        }

        private static bool HasContentPath(Item item)
        {
            return item.Paths.FullPath.StartsWith("/sitecore/content", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasRenderingsPath(Item item)
        {
            return item.Paths.FullPath.StartsWith("/sitecore/layout/renderings", StringComparison.OrdinalIgnoreCase);
        }

        private void HandleContentItem(Item item, bool itemIsDeleted, bool itemIsPublished)
        {
            if (item == null)
            {
                return;
            }

            // Skip, if the item published is not a content item
            if (!HasContentPath(item))
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

                if (!deleteResponse.Success)
                {
                    _log.Warn($"Failed deleting entity ({id}). Message: {deleteResponse.Message}", deleteResponse.Exception, this);
                }
                else
                {
                    _log.Debug($"Successfully deleting entity ({id})", this);
                }

                return;
            }

            if (itemIsPublished)
            {
                string id = _identityService.GetId(item);

                Response saveResponse = _enterspeedIngestService.Save(sitecoreContentEntity);

                if (!saveResponse.Success)
                {
                    _log.Warn($"Failed ingesting entity ({id}). Message: {saveResponse.Message}", saveResponse.Exception, this);
                }
                else
                {
                    _log.Debug($"Successfully ingested entity ({id})", this);
                }
            }
        }

        private void HandleRendering(Item item, bool itemIsDeleted, bool itemIsPublished)
        {
            if (item == null)
            {
                return;
            }

            // Skip, if the item published is not a rendering item
            if (!HasRenderingsPath(item))
            {
                return;
            }

            RenderingItem renderingItem = item;
            if (renderingItem?.InnerItem == null)
            {
                return;
            }

            if (!IsRenderingReferencedFromEnabledContent(item))
            {
                return;
            }

            SitecoreRenderingEntity sitecoreRenderingEntity = _sitecoreRenderingEntityModelMapper.Map(renderingItem);

            if (itemIsDeleted)
            {
                string id = _identityService.GetId(renderingItem);

                Response deleteResponse = _enterspeedIngestService.Delete(id);

                if (!deleteResponse.Success)
                {
                    _log.Warn($"Failed deleting entity ({id}). Message: {deleteResponse.Message}", deleteResponse.Exception, this);
                }
                else
                {
                    _log.Debug($"Successfully deleting entity ({id})", this);
                }

                return;
            }

            if (itemIsPublished)
            {
                string id = _identityService.GetId(renderingItem);

                Response saveResponse = _enterspeedIngestService.Save(sitecoreRenderingEntity);

                if (!saveResponse.Success)
                {
                    _log.Warn($"Failed ingesting entity ({id}). Message: {saveResponse.Message}", saveResponse.Exception, this);
                }
                else
                {
                    _log.Debug($"Successfully ingested entity ({id})", this);
                }
            }
        }

        private bool IsRenderingReferencedFromEnabledContent(Item item)
        {
            EnterspeedSitecoreConfiguration configuration = _enterspeedConfigurationService.GetConfiguration();

            GetLinksStrategy linksStrategy = _linkStrategyFactory.Resolve(item);

            var strategyContextArgs = new StrategyContextArgs(item);
            linksStrategy.ProcessReferrers(strategyContextArgs);

            if (strategyContextArgs.Result != null &&
                strategyContextArgs.Result.Any())
            {
                foreach (ItemLink itemLink in strategyContextArgs.Result)
                {
                    Item sourceItem = itemLink?.GetSourceItem();
                    if (sourceItem == null)
                    {
                        continue;
                    }

                    EnterspeedSiteInfo site = configuration.GetSite(sourceItem);
                    if (site != null)
                    {
                        // This rendering is referenced on content from an enabled site
                        return true;
                    }
                }
            }

            return false;
        }
    }
}