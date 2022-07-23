using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V8.Factories;
using Enterspeed.Source.SitecoreCms.V8.Handlers;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedJobsHandler : IEnterspeedJobsHandler
    {
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;
        private readonly IEnterspeedJobFactory _enterspeedJobFactory;
        private readonly IEnumerable<IEnterspeedJobHandler> _enterspeedJobHandlers;
        private readonly HttpClient _client = new HttpClient();

        public EnterspeedJobsHandler(
            IEnterspeedJobFactory enterspeedJobFactory,
            IEnterspeedSitecoreLoggingService loggingService,
            IEnterspeedJobRepository enterspeedJobRepository,
            IServiceProvider serviceProvider)
        {
            _enterspeedJobFactory = enterspeedJobFactory;
            _loggingService = loggingService;
            _enterspeedJobRepository = enterspeedJobRepository;

            if (serviceProvider != null)
            {
                _enterspeedJobHandlers = serviceProvider.GetServices<IEnterspeedJobHandler>();
            }
        }

        public void HandleJobs(IList<EnterspeedJob> jobs)
        {
            // Process nodes
            var failedJobs = new List<EnterspeedJob>();
            var failedJobsToDelete = new List<EnterspeedJob>();

            // Fetch all failed jobs for these content ids. We need to do this to delete the failed jobs if they no longer fails
            var failedJobsToHandle = _enterspeedJobRepository.GetFailedJobs(jobs.Select(x => x.EntityId).Distinct().ToList());
            var jobsByEntityIdAndContentState = jobs.GroupBy(x => new { x.EntityId, x.ContentState, x.Culture });

            // Creating a list for hooks that we have to call after data has been ingested.
            var buildHookUrls = new List<string>();

            foreach (var jobInfo in jobsByEntityIdAndContentState)
            {
                var newestJob = jobInfo
                    .OrderBy(x => x.CreatedAt)
                    .LastOrDefault();

                // We only need to execute the latest jobs instruction.
                if (newestJob == null)
                {
                    continue;
                }

                // Get the failed jobs and add it to the batch of jobs that needs to be handled, so we can delete them afterwards
                failedJobsToDelete.AddRange(failedJobsToHandle.Where(x => x.EntityId == jobInfo.Key.EntityId && x.Culture == jobInfo.Key.Culture && x.ContentState == jobInfo.Key.ContentState));

                var handler = _enterspeedJobHandlers.FirstOrDefault(f => f.CanHandle(newestJob));
                if (handler == null)
                {
                    var message = $"No job handler available for {newestJob.EntityId} {newestJob.EntityType}";
                    failedJobs.Add(_enterspeedJobFactory.GetFailedJob(newestJob, message));
                    _loggingService.Warn(message);
                    continue;
                }

                try
                {
                    handler.Handle(newestJob);
                    buildHookUrls.AddRange(newestJob.BuildHookUrls.Split(','));
                }
                catch (Exception exception)
                {   
                    var message = exception?.Message ?? "Failed to handle the job";
                    failedJobs.Add(_enterspeedJobFactory.GetFailedJob(newestJob, message));
                    _loggingService.Warn(message);
                }
            }

            if (buildHookUrls.Any())
            {
                foreach (var buildHookUrl in buildHookUrls.Distinct())
                {
                    var task = CallHookAsync(buildHookUrl);
                }
            }

            // Create all jobs that failed
            _enterspeedJobRepository.Create(failedJobs);

            // Delete all jobs - Note, that it's safe to delete all jobs because failed jobs will be created as a new job
            _enterspeedJobRepository.Delete(jobs.Select(x => x.Id).Concat(failedJobsToDelete.Select(x => x.Id)).ToList());

            // Throw exception with a combined exception message for all jobs that failed if any
            if (failedJobs.Any())
            {
                var failedJobExceptions = string.Join(Environment.NewLine, failedJobs.Select(x => x.Exception));
                throw new Exception(failedJobExceptions);
            }
        }

        private async Task<string> CallHookAsync(string path)
        {
            var result = string.Empty;

            var response = await _client.PostAsync(path, null);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}