using System;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;

namespace Enterspeed.Source.SitecoreCms.V9.Agents
{
    public class DeleteOlderEnterspeedJobs
    {
        private readonly IEnterspeedJobsHandlingService _enterspeedJobsHandlingService;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;

        public DeleteOlderEnterspeedJobs(
            IEnterspeedJobsHandlingService enterspeedJobsHandlingService,
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
