using Enterspeed.Source.SitecoreCms.V8.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V8.Handlers
{
    public interface IEnterspeedJobHandler
    {
        bool CanHandle(EnterspeedJob job);
        void Handle(EnterspeedJob job);
    }
}
