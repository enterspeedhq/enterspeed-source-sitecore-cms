using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedSitecoreIngestService
    {
        bool HasAllowedPath(Item item);

        void HandleContentItem(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview);

        void HandleRendering(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview);
        void HandleDictionary(Item item, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration, bool itemIsDeleted, bool itemIsPublished, bool itemIsPreview);

        bool IsItemReferencedFromEnabledContent(Item item, EnterspeedSitecoreConfiguration configuration);

        void DeleteEntity(RenderingItem renderingItem, EnterspeedIngestService enterspeedIngestService);
        void SaveEntity(RenderingItem renderingItem, EnterspeedIngestService enterspeedIngestService, EnterspeedSitecoreConfiguration configuration);
    }
}