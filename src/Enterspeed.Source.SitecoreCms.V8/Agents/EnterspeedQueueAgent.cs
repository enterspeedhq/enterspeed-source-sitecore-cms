using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private HttpClient _httpClient = new HttpClient();


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
            var failedJobs = _enterspeedJobRepository.GetFailedJobs();
            if (failedJobs != null)
            {
                try
                {
                    IngestContent(failedJobs);
                    _loggingService.Info("Successfully ingested all failed jobs");
                }
                catch (Exception e)
                {
                    _loggingService.Error("Something went wrong when ingesting failed jobs ", e);
                }
                finally
                {
                    RemoveIngestedJobs(failedJobs);
                }
            }

            var pendingJobs = _enterspeedJobRepository.GetPendingJobs();
            if (pendingJobs == null || !pendingJobs.Any())
            {
                return;
            }

            try
            {
                IngestContent(pendingJobs);
                _loggingService.Info("Successfully ingested all pending jobs");
            }
            catch (Exception e)
            {
                SetJobsAsFailed(pendingJobs);
                _loggingService.Error("Something went wrong when ingesting content ", e);
            }
            finally
            {
                RemoveIngestedJobs(pendingJobs);
                var buildHookUrls = pendingJobs.Select(j => j.BuildHookUrls);
                TriggerHooks(buildHookUrls);
            }
        }

        private void TriggerHooks(IEnumerable<string> buildHookUrls)
        {
            foreach (var buildHookUrl in buildHookUrls)
            {
                var task = System.Threading.Tasks.Task.Run(() => _httpClient.GetAsync(buildHookUrl));
                task.Wait();

                var response = task.Result;

                if (response.IsSuccessStatusCode)
                {
                    _loggingService.Info("Successfully triggered build hook");
                }
                else
                {
                    _loggingService.Error("Something went wrong when triggering build hook");
                }
            }

            _httpClient.Dispose();
        }

        private void SetJobsAsFailed(IList<EnterspeedJob> pendingJobs)
        {
            foreach (var job in pendingJobs)
            {
                job.State = EnterspeedJobState.Failed;
            }

            _enterspeedJobRepository.Save(pendingJobs);
        }

        private void RemoveIngestedJobs(IEnumerable<EnterspeedJob> pendingJobs)
        {
            _enterspeedJobRepository.Delete(pendingJobs.Select(p => p.Id).ToList());
        }

        private void IngestContent(IEnumerable<EnterspeedJob> pendingJobs)
        {
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

            _loggingService.Error($"Entity with id {item.ID} could not be mapped");
            return null;
        }
    }
}
