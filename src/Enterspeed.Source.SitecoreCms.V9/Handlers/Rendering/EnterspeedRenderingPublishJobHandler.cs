using System;
using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V9.Data.Models;
using Enterspeed.Source.SitecoreCms.V9.Exceptions;
using Enterspeed.Source.SitecoreCms.V9.Handlers;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V9.Services.Serializers;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Handlers.Rendering
{
    public class EnterspeedRenderingPublishJobHandler : IEnterspeedJobHandler
    {
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedGuardService _enterspeedGuardService;
        private readonly BaseItemManager _itemManager;

        public EnterspeedRenderingPublishJobHandler(
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
            return job.EntityType == EnterspeedJobEntityType.Rendering
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

        protected virtual void Ingest(IEnterspeedEntity<IDictionary<string, IEnterspeedProperty>> enterspeedData, EnterspeedJob job, EnterspeedSitecoreConfiguration configuration)
        {
            var enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new NewtonsoftJsonSerializer());

            var ingestResponse = enterspeedIngestService.Save(enterspeedData);
            if (!ingestResponse.Success)
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