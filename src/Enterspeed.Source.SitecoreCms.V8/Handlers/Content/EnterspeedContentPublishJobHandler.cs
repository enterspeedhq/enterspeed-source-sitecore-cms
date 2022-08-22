using System;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Configuration;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V8.Providers;
using Enterspeed.Source.SitecoreCms.V8.Services;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V8.Services.Serializers;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V8.Handlers.Content
{
    public class EnterspeedContentPublishJobHandler : IEnterspeedJobHandler
    {
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedGuardService _enterspeedGuardService;
        private readonly BaseItemManager _itemManager;

        public EnterspeedContentPublishJobHandler(
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            BaseItemManager itemManager,
            IEnterspeedGuardService enterspeedGuardService)
        {
            _sitecoreContentEntityModelMapper = sitecoreContentEntityModelMapper;
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _itemManager = itemManager;
            _enterspeedGuardService = enterspeedGuardService;
        }

        public virtual bool CanHandle(EnterspeedJob job)
        {
            return job.EntityType == EnterspeedJobEntityType.Content
                   && job.JobType == EnterspeedJobType.Publish
                   && job.ContentState == EnterspeedContentState.Publish;
        }

        public virtual void Handle(EnterspeedJob job)
        {
            var item = GetItem(job);
            if (!CanIngest(item, job))
            {
                return;
            }

            var configurations = _enterspeedConfigurationService.GetConfigurations();
            foreach (var configuration in configurations)
            {
                try
                {
                    var siteOfItem = configuration.GetSite(item);
                    if (siteOfItem == null)
                    {
                        continue;
                    }

                    var sitecoreEntity = _sitecoreContentEntityModelMapper.Map(item, configuration);
                    Ingest(sitecoreEntity, job, configuration);
                }
                catch (Exception e)
                {
                    throw new JobHandlingException(
                        $"Failed creating entity ({job.EntityId}/{job.Culture}). Message: {e.Message}. StackTrace: {e.StackTrace}");
                }
            }
        }

        protected virtual Item GetItem(EnterspeedJob job)
        {
            var isItemId = ID.TryParse(job.EntityId, out var itemId);
            var item = isItemId
                ? _itemManager.GetItem(itemId, Language.Parse(job.Culture), Sitecore.Data.Version.Latest, Database.GetDatabase("master"))
                : null;

            if (item == null)
            {
                throw new JobHandlingException($"Item with id {job.EntityId} does not exist");
            }

            return item;
        }

        protected virtual bool CanIngest(Item item, EnterspeedJob job)
        {
            return _enterspeedGuardService.CanIngest(item, job.Culture);
        }

        protected virtual void Ingest(IEnterspeedEntity enterspeedData, EnterspeedJob job, EnterspeedConfiguration configuration)
        {
            var enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new NewtonsoftJsonSerializer(), new EnterspeedSitecoreConfigurationProvider(_enterspeedConfigurationService));
            var ingestResponse = enterspeedIngestService.Save(enterspeedData); if (!ingestResponse.Success)
            {
                var message = ingestResponse.Exception != null
                    ? ingestResponse.Exception.Message
                    : ingestResponse.Message;
                throw new JobHandlingException(
                    $"Failed ingesting entity ({job.EntityId}/{job.Culture}). Message: {message}");
            }
        }
    }
}