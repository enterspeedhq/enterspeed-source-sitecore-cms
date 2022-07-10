using System;
using System.Net;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Enterspeed.Source.SitecoreCms.V8.Exceptions;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V8.Handlers.PreviewContent
{
    public class EnterspeedPreviewContentDeleteJobHandler : IEnterspeedJobHandler
    {
        private readonly IEnterspeedIngestService _enterspeedIngestService;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public EnterspeedPreviewContentDeleteJobHandler(
            IEnterspeedIngestService enterspeedIngestService,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _enterspeedIngestService = enterspeedIngestService;
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public bool CanHandle(EnterspeedJob job)
        {
            return
                job.ContentState == EnterspeedContentState.Preview
                && job.EntityType == EnterspeedJobEntityType.Content
                && job.JobType == EnterspeedJobType.Delete;
        }

        public void Handle(EnterspeedJob job)
        {
            var id = _enterspeedIdentityService.GetId(Guid.Parse(job.EntityId), Language.Parse(job.Culture));
            var deleteResponse = _enterspeedIngestService.Delete(id);
            if (!deleteResponse.Success && deleteResponse.Status != HttpStatusCode.NotFound)
            {
                throw new JobHandlingException($"Failed deleting entity ({job.EntityId}/{job.Culture}). Message: {deleteResponse.Message}");
            }
        }
    }
}