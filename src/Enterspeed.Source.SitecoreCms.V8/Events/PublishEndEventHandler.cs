using System;
using System.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Jobs;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class PublishEndEventHandler
    {
        private readonly IEnterspeedJobsHandlingService _enterspeedJobsHandlingService;

        public PublishEndEventHandler(
             IEnterspeedJobsHandlingService enterspeedJobsHandlingService)
        {
            _enterspeedJobsHandlingService = enterspeedJobsHandlingService;
        }

        public void PublishEnd(object sender, EventArgs args)
        {
            var batchSizeConfig = ConfigurationManager.AppSettings["enterspeedBatchSize"];
            var batchSizeParsed = int.TryParse(batchSizeConfig, out var batchSize);

            object[] parameters = { batchSizeParsed ? 2000 : batchSize };

            var jobOptions = new JobOptions("handleQueuedJobs", "Enterspeed", "CM", _enterspeedJobsHandlingService,
                "HandlePendingJobs", parameters)
            {
                WriteToLog = true
            };

            var job = new Job(jobOptions);
            JobManager.Start(job);
        }
    }
}