using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class BaseEnterspeedEventHandler
    {
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedJobsHandlingService _enterspeedJobsHandlingService;

        public BaseEnterspeedEventHandler(IEnterspeedJobRepository enterspeedJobRepository,
            IEnterspeedJobsHandlingService enterspeedJobsHandlingService)
        {
            _enterspeedJobRepository = enterspeedJobRepository;
            _enterspeedJobsHandlingService = enterspeedJobsHandlingService;
        }

        public void EnqueueJobs(IList<EnterspeedJob> jobs)
        {
            if (!jobs.Any())
            {
                return;
            }

            _enterspeedJobRepository.Save(jobs);
        }

        protected void HandleJobs(IList<EnterspeedJob> jobs)
        {
            _enterspeedJobsHandlingService.HandleJobs(jobs);
        }
    }
}