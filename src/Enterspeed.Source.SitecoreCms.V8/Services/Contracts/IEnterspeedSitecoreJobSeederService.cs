using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedSitecoreJobSeederService
    {
        bool HasAllowedPath(Item item);
        void HandleContentItem(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished);
        void HandleRendering(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished);
        void HandleDictionary(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished);
    }
}