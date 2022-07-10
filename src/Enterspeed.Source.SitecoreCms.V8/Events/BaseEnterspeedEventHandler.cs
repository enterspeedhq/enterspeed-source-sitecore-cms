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

        public BaseEnterspeedEventHandler(IEnterspeedJobRepository enterspeedJobRepository)
        {
            _enterspeedJobRepository = enterspeedJobRepository;
        }

        public void EnqueueJobs(IList<EnterspeedJob> jobs)
        {
            if (!jobs.Any())
            {
                return;
            }

            _enterspeedJobRepository.Save(jobs);
        }
    }
}