using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedJobsHandler
    {
        void HandleJobs(IList<EnterspeedJob> jobs);
    }
}