using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V8.Handlers.Dictionaries
{
    public class EnterspeedDictionaryItemPublishJobHandler : IEnterspeedJobHandler
    {
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEnterspeedGuardService _enterspeedGuardService;
        private readonly IEntityModelMapper<Item, SitecoreDictionaryEntity> _sitecoreDictionaryEntityModelMapper;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly BaseItemManager _itemManager;


        public EnterspeedDictionaryItemPublishJobHandler(
            IEnterspeedIngestService enterspeedIngestService,
            IEnterspeedGuardService enterspeedGuardService,
            IEntityModelMapper<Item, SitecoreDictionaryEntity> sitecoreDictionaryEntityModelMapper,
            IEnterspeedConfigurationService enterspeedConfigurationService, BaseItemManager itemManager)
        {
            _enterspeedIngestService = enterspeedIngestService;
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


            var sitecoreDictionaryEntities = CreateSitecoreDictionaryEntity(dictionaryItem, job);
            foreach (var entity in sitecoreDictionaryEntities)
            {
                Ingest(entity, job);
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

        protected virtual IList<SitecoreDictionaryEntity> CreateSitecoreDictionaryEntity(Item item, EnterspeedJob job)
        {
            var dictionaryEntities = new List<SitecoreDictionaryEntity>();

            try
            {
                var configurations = _enterspeedConfigurationService.GetConfigurations();
                foreach (var configuration in configurations)
                {
                    if (!configuration.SiteInfos.Any(s => s.IsDictionaryOfSite(item)))
                    {
                        continue;
                    }

                    var dictionaryEntity = _sitecoreDictionaryEntityModelMapper.Map(item, configuration);
                    dictionaryEntities.Add(dictionaryEntity);
                }
            }
            catch (Exception e)
            {
                throw new JobHandlingException($"Failed creating entity ({job.EntityId}/{job.Culture}). Message: {e.Message}. StackTrace: {e.StackTrace}");
            }

            return dictionaryEntities;
        }

        protected virtual void Ingest(IEnterspeedEntity sitecoreData, EnterspeedJob job)
        {
            var ingestResponse = _enterspeedIngestService.Save(sitecoreData);
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