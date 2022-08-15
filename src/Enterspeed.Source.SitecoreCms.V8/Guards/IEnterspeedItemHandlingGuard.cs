using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Guards
{
    public interface IEnterspeedItemHandlingGuard
    {
        /// <summary>
        /// Validates if item can be ingested.
        /// </summary>
        /// <param name="item">Content for ingest.</param>
        /// <param name="culture">Culture of item.</param>
        /// <returns>True or false, if is valid for ingest or not.</returns>
        bool CanIngest(Item item, string culture);
    }
}
