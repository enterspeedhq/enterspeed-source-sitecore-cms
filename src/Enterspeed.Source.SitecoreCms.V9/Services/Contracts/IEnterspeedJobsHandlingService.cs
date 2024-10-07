using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V9.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V9.Services.Contracts
{
    public interface IEnterspeedJobsHandlingService
    {
        void HandleJobs(IList<EnterspeedJob> jobs);
        void HandlePendingJobs(int batchSize = 2000);
        void InvalidateOldProcessingJobs();
    }
}