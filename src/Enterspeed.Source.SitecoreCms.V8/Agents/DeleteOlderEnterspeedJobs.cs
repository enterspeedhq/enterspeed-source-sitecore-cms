using System;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Tasks;

namespace Enterspeed.Source.SitecoreCms.V8.Agents
{
    public class DeleteOlderEnterspeedJobs : BaseAgent
    {
        private readonly IEnterspeedJobsHandlingService _enterspeedJobsHandlingService;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;

        public DeleteOlderEnterspeedJobs(IEnterspeedJobsHandlingService enterspeedJobsHandlingService,
            IEnterspeedSitecoreLoggingService loggingService)
        {
            _enterspeedJobsHandlingService = enterspeedJobsHandlingService;
            _loggingService = loggingService;
        }

        public void Run()
        {
            try
            {
                _enterspeedJobsHandlingService.InvalidateOldProcessingJobs();
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong when handling jobs ", e);
            }

        }
    }
}
