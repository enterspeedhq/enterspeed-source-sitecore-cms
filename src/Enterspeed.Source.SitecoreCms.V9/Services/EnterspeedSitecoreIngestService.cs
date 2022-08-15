using System;
using System.Diagnostics;
using System.Linq;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V9.Extensions;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Links;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedSitecoreIngestService : IEnterspeedSitecoreIngestService
    {
        private readonly BaseLinkStrategyFactory _linkStrategyFactory;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> _sitecoreRenderingEntityModelMapper;
        private readonly IEntityModelMapper<Item, SitecoreDictionaryEntity> _sitecoreDictionaryEntityModelMapper;
        private readonly IEnterspeedIdentityService _identityService;
        public EnterspeedSitecoreIngestService(
            BaseItemManager itemManager,
            BaseLinkStrategyFactory linkStrategyFactory,
            IEnterspeedSitecoreLoggingService loggingService,
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> sitecoreRenderingEntityModelMapper,
            IEntityModelMapper<Item, SitecoreDictionaryEntity> sitecoreDictionaryEntityModelMapper,
            IEnterspeedIdentityService identityService)
        {
            _linkStrategyFactory = linkStrategyFactory;
            _loggingService = loggingService;
            _sitecoreContentEntityModelMapper = sitecoreContentEntityModelMapper;
            _sitecoreRenderingEntityModelMapper = sitecoreRenderingEntityModelMapper;
            _sitecoreDictionaryEntityModelMapper = sitecoreDictionaryEntityModelMapper;
            _identityService = identityService;
        }

        public bool HasAllowedPath(Item item)
        {
            return item.IsContentItem() || item.IsRenderingItem() || item.IsDictionaryItem();
        }

        public void HandleContentItem(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview)
        {
            try
            {
                if (item == null)
                {
                    return;
                }

                // Skip, if the item published is not a content item
                if (!item.IsContentItem())
                {
                    return;
                }

                EnterspeedSiteInfo siteOfItem = configuration.GetSite(item);
                if (siteOfItem == null)
                {
                    _loggingService.Warn($"No site config for  ({item.ID})");
                    // If no enabled site was found for this item, skip it
                    return;
                }

                SitecoreContentEntity sitecoreContentEntity = _sitecoreContentEntityModelMapper.Map(item, configuration);
                if (sitecoreContentEntity == null)
                {
                    return;
                }

                if (itemIsDeleted)
                {
                    string id = _identityService.GetId(item);
                    _loggingService.Info($"Beginning to delete content entity ({id}).");
                    Response deleteResponse = enterspeedIngestService.Delete(id);

                    if (!deleteResponse.Success)
                    {
                        _loggingService.Warn($"Failed deleting content entity ({id}). Message: {deleteResponse.Message}", deleteResponse.Exception);
                    }
                    else
                    {
                        _loggingService.Debug($"Successfully deleting content entity ({id})");
                    }

                    return;
                }

                if (itemIsPublished)
                {
                    string id = _identityService.GetId(item);
                    _loggingService.Info($"Beginning to ingest content entity ({id}).");
                    Response saveResponse = enterspeedIngestService.Save(sitecoreContentEntity);

                    if (!saveResponse.Success)
                    {
                        _loggingService.Warn($"Failed ingesting content entity ({id}). Message: {saveResponse.Message}", saveResponse.Exception);
                    }
                    else
                    {
                        _loggingService.Debug($"Successfully ingested content entity ({id})");
                    }
                }
            }
            catch (Exception exception)
            {
                _loggingService.Error("An error happened: ", exception);
                throw;
            }
        }

        public void HandleRendering(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview)
        {
            try
            {
                if (item == null)
                {
                    return;
                }

                // Skip, if the item published is not a rendering item
                if (!item.IsRenderingItem())
                {
                    return;
                }

                RenderingItem renderingItem = item;
                if (renderingItem?.InnerItem == null)
                {
                    return;
                }

                if (!IsItemReferencedFromEnabledContent(item, configuration))
                {
                    return;
                }

                if (itemIsDeleted)
                {
                    DeleteEntity(renderingItem, enterspeedIngestService);
                }
                else if (itemIsPublished)
                {
                    SaveEntity(renderingItem, enterspeedIngestService, configuration);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        public void HandleDictionary(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview)
        {
            if (item == null)
            {
                return;
            }

            // Skip, if the item published is not a dictionary item
            if (!item.IsDictionaryItem())
            {
                return;
            }

            if (!configuration.SiteInfos.Any(s => s.IsDictionaryOfSite(item)))
            {
                return;
            }

            SitecoreDictionaryEntity sitecoreDictionaryEntity = _sitecoreDictionaryEntityModelMapper.Map(item, configuration);
            if (sitecoreDictionaryEntity == null)
            {
                return;
            }

            if (itemIsDeleted)
            {
                string id = _identityService.GetId(item);
                _loggingService.Info($"Beginning to delete dictionary entity ({id}).");
                Response deleteResponse = enterspeedIngestService.Delete(id);

                if (!deleteResponse.Success)
                {
                    _loggingService.Warn($"Failed deleting dictionary entity ({id}). Message: {deleteResponse.Message}", deleteResponse.Exception);
                }
                else
                {
                    _loggingService.Debug($"Successfully deleting dictionary entity ({id})");
                }

                return;
            }

            if (itemIsPublished)
            {
                string id = _identityService.GetId(item);
                _loggingService.Info($"Beginning to ingest dictionary entity ({id}).");
                Response saveResponse = enterspeedIngestService.Save(sitecoreDictionaryEntity);

                if (!saveResponse.Success)
                {
                    _loggingService.Warn($"Failed ingesting dictionary entity ({id}). Message: {saveResponse.Message}", saveResponse.Exception);
                }
                else
                {
                    _loggingService.Debug($"Successfully ingested dictionary entity ({id})");
                }
            }
        }

        public bool IsItemReferencedFromEnabledContent(Item item, EnterspeedSitecoreConfiguration configuration)
        {
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

        public void DeleteEntity(RenderingItem renderingItem, EnterspeedIngestService enterspeedIngestService)
        {
            string id = _identityService.GetId(renderingItem);
            _loggingService.Info($"Beginning to delete rendering entity ({id}).");
            Response deleteResponse = enterspeedIngestService.Delete(id);

            if (!deleteResponse.Success)
            {
                _loggingService.Warn($"Failed deleting rendering entity ({id}). Message: {deleteResponse.Message}", deleteResponse.Exception);
            }
            else
            {
                _loggingService.Debug($"Successfully deleting rendering entity ({id})");
            }
        }

        public void SaveEntity(RenderingItem renderingItem, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration)
        {
            SitecoreRenderingEntity sitecoreRenderingEntity = _sitecoreRenderingEntityModelMapper.Map(renderingItem, configuration);

            string id = _identityService.GetId(renderingItem);
            _loggingService.Info($"Beginning to ingest rendering entity ({id}).");
            Response saveResponse = enterspeedIngestService.Save(sitecoreRenderingEntity);

            if (!saveResponse.Success)
            {
                _loggingService.Warn($"Failed ingesting rendering entity ({id}). Message: {saveResponse.Message}", saveResponse.Exception);
            }
            else
            {
                _loggingService.Debug($"Successfully ingested rendering entity ({id})");
            }
        }
    }
}