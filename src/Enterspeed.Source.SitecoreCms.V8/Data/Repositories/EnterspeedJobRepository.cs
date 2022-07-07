using System;
using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V8.Data.Repositories
{
    public class EnterspeedJobRepository : IEnterspeedJobRepository
    {
        public EnterspeedJobRepository()
        {
        }

        public IList<EnterspeedJob> GetFailedJobs()
        {
            throw new NotImplementedException();
        }

        public IList<EnterspeedJob> GetFailedJobs(IList<string> entityIds)
        {
            throw new NotImplementedException();
        }

        public IList<EnterspeedJob> GetPendingJobs()
        {
            throw new NotImplementedException();
        }

        public IList<EnterspeedJob> GetOldProcessingTasks(int olderThanMinutes = 60)
        {
            throw new NotImplementedException();
        }

        public void Save(IList<EnterspeedJob> jobs)
        {
            throw new NotImplementedException();
        }

        public void Delete(IList<int> ids)
        {
            throw new NotImplementedException();
        }
    }
}