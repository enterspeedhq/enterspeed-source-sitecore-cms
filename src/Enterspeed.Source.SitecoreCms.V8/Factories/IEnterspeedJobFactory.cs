using Enterspeed.Source.SitecoreCms.V8.Data.Models;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Factories
{
    public interface IEnterspeedJobFactory
    {
        EnterspeedJob GetPublishJob(Item content, string culture, EnterspeedContentState state);
        EnterspeedJob GetDeleteJob(Item content, string culture, EnterspeedContentState state);
        EnterspeedJob GetFailedJob(EnterspeedJob job, string exception);
    }
}