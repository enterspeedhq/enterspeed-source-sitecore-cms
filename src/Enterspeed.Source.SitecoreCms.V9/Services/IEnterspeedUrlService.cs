using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public interface IEnterspeedUrlService
    {
        string GetItemUrl(Item item, bool enableLanguageEmbedding = false);

        string GetMediaUrl(MediaItem mediaItem);
    }
}