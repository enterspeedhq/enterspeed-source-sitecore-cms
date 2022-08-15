using System;
using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Factories
{
    public class EnterspeedJobFactory : IEnterspeedJobFactory
    {
        public EnterspeedJob GetPublishJob(Item content, string culture, EnterspeedContentState state, EnterspeedJobEntityType? enterspeedJobEntityType = null, IEnumerable<string> publishHookUrls = null)
        {
            var now = DateTime.UtcNow;
            var job = new EnterspeedJob
            {
                EntityId = content.ID.ToString(),
                EntityType = EnterspeedJobEntityType.Content,
                Culture = culture,
                JobType = EnterspeedJobType.Publish,
                State = EnterspeedJobState.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                ContentState = state
            };

            if (enterspeedJobEntityType != null)
            {
                job.EntityType = enterspeedJobEntityType.Value;
            }

            return job;
        }

        public EnterspeedJob GetDeleteJob(Item content, string culture, EnterspeedContentState state, EnterspeedJobEntityType? enterspeedJobEntityType = null, IEnumerable<string> publishHookUrls = null)
        {
            var now = DateTime.UtcNow;
            var job = new EnterspeedJob
            {
                EntityId = content.ID.ToString(),
                EntityType = EnterspeedJobEntityType.Content,
                Culture = culture,
                JobType = EnterspeedJobType.Delete,
                State = EnterspeedJobState.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                ContentState = state
            };

            if (enterspeedJobEntityType != null)
            {
                job.EntityType = enterspeedJobEntityType.Value;
            }

            return job;
        }

        public EnterspeedJob GetFailedJob(EnterspeedJob job, string exception)
        {
            return new EnterspeedJob
            {
                EntityId = job.EntityId,
                EntityType = job.EntityType,
                Culture = job.Culture,
                CreatedAt = job.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                JobType = job.JobType,
                State = EnterspeedJobState.Failed,
                ContentState = job.ContentState,
                Exception = exception
            };
        }
    }
}