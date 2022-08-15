using System;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedIdentityService
    {
        string GetId(Item item);
        string GetId(RenderingItem renderingItem);
        string GetId(Guid itemId, Language language);
        EnterspeedSitecoreIdentity Parse(string id);
    }
}