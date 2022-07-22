using System;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Tasks;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Agents
{
    public class EnterspeedQueueAgent : BaseAgent
    {
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _sitecoreContentEntityModelMapper;
        private readonly IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> _sitecoreRenderingEntityModelMapper;
        private readonly IEntityModelMapper<Item, SitecoreDictionaryEntity> _sitecoreDictionaryEntityModelMapper;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;
        private readonly BaseItemManager _itemManager;

        public EnterspeedQueueAgent(IEnterspeedJobRepository enterspeedJobRepository,
            IEnterspeedIngestService enterspeedIngestService,
            IEntityModelMapper<Item, SitecoreDictionaryEntity> sitecoreDictionaryEntityModelMapper,
            IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> sitecoreRenderingEntityModelMapper,
            IEntityModelMapper<Item, SitecoreContentEntity> sitecoreContentEntityModelMapper,
            BaseItemManager itemManager,
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedSitecoreLoggingService loggingService)
        {
            _enterspeedJobRepository = enterspeedJobRepository;
            _enterspeedIngestService = enterspeedIngestService;
            _sitecoreDictionaryEntityModelMapper = sitecoreDictionaryEntityModelMapper;
            _sitecoreRenderingEntityModelMapper = sitecoreRenderingEntityModelMapper;
            _sitecoreContentEntityModelMapper = sitecoreContentEntityModelMapper;
            _itemManager = itemManager;
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _loggingService = loggingService;
        }

        public void Run()
        {
            var pendingJobs = _enterspeedJobRepository.GetPendingJobs();
            if (pendingJobs == null || !pendingJobs.Any())
            {
                return;
            }

            var configurations = _enterspeedConfigurationService.GetConfigurations();
            foreach (var job in pendingJobs)
            {
                var item = _itemManager.GetItem(ID.Parse(job.EntityId), Language.Parse(job.Culture), Version.Latest, Factory.GetDatabase("Master"));
                foreach (var configuration in configurations)
                {
                    try
                    {
                        var siteOfItem = configuration.GetSite(item);
                        if (siteOfItem == null)
                        {
                            return;
                        }

                        var entity = MapEnterspeedEntity(job, item, configuration);
                        if (entity != null)
                        {
                            _enterspeedIngestService.Save(entity);
                            _loggingService.Info("Successfully ingested content with the id of " + entity.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _loggingService.Error("Something went wrong when ingesting content ", e);
                    }
                }
            }
        }

        private IEnterspeedEntity MapEnterspeedEntity(EnterspeedJob job, Item item, EnterspeedSitecoreConfiguration configuration)
        {
            switch (job.EntityType)
            {
                case EnterspeedJobEntityType.Dictionary:
                    return _sitecoreDictionaryEntityModelMapper.Map(item, configuration);
                case EnterspeedJobEntityType.Content:
                    return _sitecoreContentEntityModelMapper.Map(item, configuration);
                case EnterspeedJobEntityType.Rendering:
                    return _sitecoreRenderingEntityModelMapper.Map(item, configuration);
            }

            return null;
        }
    }
}