using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V9.Data.Models;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Factories
{
    public interface IEnterspeedJobFactory
    {
        EnterspeedJob GetPublishJob(Item content, string culture, EnterspeedContentState state, EnterspeedJobEntityType? enterspeedJobEntityType = null, IEnumerable<string> publishHookUrls = null);
        EnterspeedJob GetDeleteJob(Item content, string culture, EnterspeedContentState state, EnterspeedJobEntityType? enterspeedJobEntityType = null, IEnumerable<string> publishHookUrls = null);
        EnterspeedJob GetFailedJob(EnterspeedJob job, string exception);
    }
}