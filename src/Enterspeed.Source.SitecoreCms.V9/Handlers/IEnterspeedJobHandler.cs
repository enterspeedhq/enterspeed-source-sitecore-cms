using Enterspeed.Source.SitecoreCms.V9.Data.Models;

namespace Enterspeed.Source.SitecoreCms.V9.Handlers
{
    public interface IEnterspeedJobHandler
    {
        bool CanHandle(EnterspeedJob job);
        void Handle(EnterspeedJob job);
    }
}
