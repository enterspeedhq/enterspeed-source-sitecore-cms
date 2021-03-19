using System;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public interface IEnterspeedIdentityService
    {
        string GetId(Item item);
        string GetId(RenderingItem renderingItem);
        string GetId(Guid itemId, Language language);
    }
}