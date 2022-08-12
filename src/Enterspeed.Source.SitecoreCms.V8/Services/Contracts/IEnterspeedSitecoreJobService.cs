using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedSitecoreJobService
    {
        void Seed(Item item);
        bool HasAllowedPath(Item item);
        void HandleContentItem(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null);
        void HandleRendering(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null);
        void HandleDictionary(Item item, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, Language overrideLanguage = null);
    }
}