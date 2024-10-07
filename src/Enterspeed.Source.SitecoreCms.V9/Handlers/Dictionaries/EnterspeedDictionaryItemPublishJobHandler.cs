using System;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Configuration;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V9.Data.Models;
using Enterspeed.Source.SitecoreCms.V9.Exceptions;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V9.Services.Serializers;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Handlers.Dictionaries
{
    public class EnterspeedDictionaryItemPublishJobHandler : IEnterspeedJobHandler
    {
        private readonly IEnterspeedGuardService _enterspeedGuardService;
        private readonly IEntityModelMapper<Item, SitecoreDictionaryEntity> _sitecoreDictionaryEntityModelMapper;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly BaseItemManager _itemManager;

        public EnterspeedDictionaryItemPublishJobHandler(
            IEnterspeedGuardService enterspeedGuardService,
            IEntityModelMapper<Item, SitecoreDictionaryEntity> sitecoreDictionaryEntityModelMapper,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            BaseItemManager itemManager)
        {
            _enterspeedGuardService = enterspeedGuardService;
            _sitecoreDictionaryEntityModelMapper = sitecoreDictionaryEntityModelMapper;
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _itemManager = itemManager;
        }

        public bool CanHandle(EnterspeedJob job)
        {
            return job.EntityType == EnterspeedJobEntityType.Dictionary
                && job.ContentState == EnterspeedContentState.Publish
                && job.JobType == EnterspeedJobType.Publish;
        }

        public void Handle(EnterspeedJob job)
        {
            var dictionaryItem = GetDictionaryItem(job);
            if (!CanIngest(dictionaryItem, job))
            {
                return;
            }

            var configurations = _enterspeedConfigurationService.GetConfigurations();
            foreach (var configuration in configurations)
            {
                try
                {
                    var sitecoreEntity = _sitecoreDictionaryEntityModelMapper.Map(dictionaryItem, configuration);
                    Ingest(sitecoreEntity, job, configuration);
                }
                catch (Exception e)
                {
                    throw new JobHandlingException(
                        $"Failed creating entity ({job.EntityId}/{job.Culture}). Message: {e.Message}. StackTrace: {e.StackTrace}");
                }
            }
        }

        protected virtual Item GetDictionaryItem(EnterspeedJob job)
        {
            var isDictionaryId = ID.TryParse(job.EntityId, out var itemId);
            var dictionaryItem = isDictionaryId ? _itemManager.GetItem(itemId, Language.Parse(job.Culture), Sitecore.Data.Version.Latest, Database.GetDatabase("master"))
                : null;

            if (dictionaryItem == null)
            {
                throw new JobHandlingException($"Dictionary with id {job.EntityId} not in database");
            }

            return dictionaryItem;
        }

        protected virtual bool CanIngest(Item dictionaryItem, EnterspeedJob job)
        {
            // Check if any of guards are against it
            return _enterspeedGuardService.CanIngest(dictionaryItem, job.Culture);
        }

        protected virtual void Ingest(IEnterspeedEntity sitecoreData, EnterspeedJob job, EnterspeedConfiguration configuration)
        {
            var enterspeedIngestService = new EnterspeedIngestService(new SitecoreEnterspeedConnection(configuration), new NewtonsoftJsonSerializer(), new EnterspeedSitecoreConfigurationProvider(_enterspeedConfigurationService));

            var ingestResponse = enterspeedIngestService.Save(sitecoreData);
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