using System;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedSitecoreIdentityService : IEnterspeedIdentityService
    {
        private readonly BaseLanguageManager _languageManager;

        public EnterspeedSitecoreIdentityService(
            BaseLanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        public string GetId(Item item)
        {
            if (item == null)
            {
                return null;
            }

            return GetId(item.ID.Guid, item.Language);
        }

        public string GetId(RenderingItem renderingItem)
        {
            if (renderingItem == null)
            {
                return null;
            }

            return $"{renderingItem.ID.Guid:N}";
        }

        public string GetId(Guid itemId, Language language)
        {
            if (language == null)
            {
                return $"{itemId:N}";
            }

            return $"{itemId:N}-{language.Name}";
        }

        public EnterspeedSitecoreIdentity Parse(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            Language language = _languageManager.GetDefaultLanguage();

            if (Guid.TryParse(id, out var itemId))
            {
                return new EnterspeedSitecoreIdentity
                {
                    ID = new ID(itemId),
                    Language = language
                };
            }

            if (id.Contains('-'))
            {
                string[] idAndCulture = id.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (idAndCulture.Length == 0)
                {
                    return null;
                }

                if (!Guid.TryParse(idAndCulture[0], out itemId))
                {
                    return null;
                }

                if (idAndCulture.Length == 2)
                {
                    itemId = Guid.Parse(idAndCulture[0]);
                    language = _languageManager.GetLanguage(idAndCulture[1]);
                }

                return new EnterspeedSitecoreIdentity
                {
                    ID = new ID(itemId),
                    Language = language
                };
            }

            if (!Guid.TryParse(id, out itemId))
            {
                return null;
            }

            return new EnterspeedSitecoreIdentity
            {
                ID = new ID(itemId),
                Language = language
            };
        }
    }
}