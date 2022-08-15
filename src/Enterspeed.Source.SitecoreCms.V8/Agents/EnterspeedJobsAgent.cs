using System;
using System.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Tasks;

namespace Enterspeed.Source.SitecoreCms.V8.Agents
{
    public class EnterspeedJobsAgent : BaseAgent
    {
        private readonly IEnterspeedJobsHandlingService _enterspeedJobsHandlingService;
        private readonly IEnterspeedSitecoreLoggingService _loggingService;

        public EnterspeedJobsAgent(IEnterspeedJobsHandlingService enterspeedJobsHandlingService,
            IEnterspeedSitecoreLoggingService loggingService)
        {
            _enterspeedJobsHandlingService = enterspeedJobsHandlingService;
            _loggingService = loggingService;
        }

        public void Run()
        {
            try
            {
                var batchSizeConfig = ConfigurationManager.AppSettings["enterspeedBatchSize"];
                var batchSizeParsed = int.TryParse(batchSizeConfig, out var batchSize);

                _enterspeedJobsHandlingService.HandlePendingJobs(batchSizeParsed ? batchSize : 2000);
            }
            catch (Exception e)
            {
                _loggingService.Error("Something went wrong when handling jobs ", e);
            }
        }
    }
}
