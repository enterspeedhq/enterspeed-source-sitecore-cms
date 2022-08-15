using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls.Expressions;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Factories;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedSitecoreJobService : IEnterspeedSitecoreJobService
    {
        private readonly BaseLinkStrategyFactory _linkStrategyFactory;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEnterspeedJobFactory _enterspeedJobFactory;
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;


        public EnterspeedSitecoreJobService(
            BaseLinkStrategyFactory linkStrategyFactory,
            IEnterspeedSitecoreLoggingService loggingService,
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            IEnterspeedJobFactory enterspeedJobFactory,
            IEnterspeedJobRepository enterspeedJobRepository,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _linkStrategyFactory = linkStrategyFactory;
            _loggingService = loggingService;
            _sitecoreContentEntityModelMapper = sitecoreContentEntityModelMapper;
            _enterspeedJobFactory = enterspeedJobFactory;
            _enterspeedJobRepository = enterspeedJobRepository;
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public void Seed(Item item)
        {
            var database = Sitecore.Configuration.Factory.GetDatabase("web");
            var targetItem = database.GetItem(item.ID, item.Language);
            if (targetItem != null)
            {
                var descendants = targetItem.Axes.GetDescendants();
                foreach (var configuration in _enterspeedConfigurationService.GetConfigurations())
                {
                    foreach (var descendant in descendants)
                    {
                        HandleContentItem(descendant, configuration, false, true);
                        HandleRendering(descendant, configuration, false, true);
                        HandleDictionary(descendant, configuration, false, true);
                    }
                }
            }
        }

        public bool HasAllowedPath(Item item)
        {
            return item.IsContentItem() || item.IsRenderingItem() || item.IsDictionaryItem();
        }

        public void HandleContentItem(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null)
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

                var siteOfItem = configuration.GetSite(item);
                if (siteOfItem == null)
                {
                    return;
                }

                var sitecoreContentEntity = _sitecoreContentEntityModelMapper.Map(item, configuration);
                if (sitecoreContentEntity == null)
                {
                    return;
                }

                var language = overrideLanguage != null ? overrideLanguage.Name : item.Language.Name;

                if (itemIsDeleted)
                {
                    var job = _enterspeedJobFactory.GetDeleteJob(item, language, EnterspeedContentState.Publish, publishHookUrls: configuration.SiteInfos.Select(s => s.PublishHookUrl));
                    EnqueueJob(job);
                }

                if (itemIsPublished)
                {
                    var job = _enterspeedJobFactory.GetPublishJob(item, language, EnterspeedContentState.Publish, publishHookUrls: configuration.SiteInfos.Select(s => s.PublishHookUrl));
                    EnqueueJob(job);
                }
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong when handling content item: ", e);
            }
        }

        public void HandleRendering(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null)
        {
            try
            {
                if (!IsRenderingItem(item))
                {
                    return;
                }

                if (!IsItemReferencedFromEnabledContent(item, configuration))
                {
                    return;
                }

                var language = overrideLanguage != null ? overrideLanguage.Name : item.Language.Name;

                if (itemIsDeleted)
                {
                    var job = _enterspeedJobFactory.GetDeleteJob(item, language, EnterspeedContentState.Publish, EnterspeedJobEntityType.Rendering, configuration.SiteInfos.Select(s => s.PublishHookUrl));
                    EnqueueJob(job);
                }
                else if (itemIsPublished)
                {
                    var job = _enterspeedJobFactory.GetPublishJob(item, language, EnterspeedContentState.Publish, EnterspeedJobEntityType.Rendering, configuration.SiteInfos.Select(s => s.PublishHookUrl));
                    EnqueueJob(job);
                }
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong when handling rendering item: ", e);
            }
        }

        public void HandleDictionary(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null)
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

            var language = overrideLanguage != null ? overrideLanguage.Name : item.Language.Name;

            if (itemIsDeleted)
            {
                var job = _enterspeedJobFactory.GetDeleteJob(item, language, EnterspeedContentState.Publish, EnterspeedJobEntityType.Dictionary, configuration.SiteInfos.Select(s => s.PublishHookUrl));
                EnqueueJob(job);
            }

            if (itemIsPublished)
            {
                var job = _enterspeedJobFactory.GetPublishJob(item, language, EnterspeedContentState.Publish, EnterspeedJobEntityType.Dictionary, configuration.SiteInfos.Select(s => s.PublishHookUrl));
                EnqueueJob(job);
            }
        }

        private bool IsItemReferencedFromEnabledContent(Item item, EnterspeedSitecoreConfiguration configuration)
        {
            var linksStrategy = _linkStrategyFactory.Resolve(item);

            var strategyContextArgs = new StrategyContextArgs(item);
            linksStrategy.ProcessReferrers(strategyContextArgs);

            if (strategyContextArgs.Result != null &&
                strategyContextArgs.Result.Any())
            {
                foreach (var itemLink in strategyContextArgs.Result)
                {
                    var sourceItem = itemLink?.GetSourceItem();
                    if (sourceItem == null)
                    {
                        continue;
                    }

                    var site = configuration.GetSite(sourceItem);
                    if (site != null)
                    {
                        // This rendering is referenced on content from an enabled site
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRenderingItem(Item item)
        {
            RenderingItem renderingItem = item;
            return item.IsRenderingItem() && renderingItem.InnerItem != null;
        }

        private void EnqueueJob(EnterspeedJob job)
        {
            _enterspeedJobRepository.Create(job);
        }
    }
}