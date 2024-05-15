using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V9.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V9.Services.Contracts
{
    public interface IEnterspeedJobsHandler
    {
        void HandleJobs(IList<EnterspeedJob> jobs);
    }
}