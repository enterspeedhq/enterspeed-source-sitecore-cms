using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V8.Data.Repositories
{
    public interface IEnterspeedJobRepository
    {
        IList<EnterspeedJob> GetFailedJobs();
        IList<EnterspeedJob> GetPendingJobs(int count = 60);
        IList<EnterspeedJob> GetOldProcessingTasks(int olderThanMinutes = 60);
        void Create(EnterspeedJob jobs);
        void Create(IList<EnterspeedJob> jobs);
        void Delete(IList<int> ids);
        IList<EnterspeedJob> GetFailedJobs(List<string> entityIds);
        void Update(IList<EnterspeedJob> jobs);
    }
}