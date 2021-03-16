using System;
using Enterspeed.Source.SitecoreCms.V9.Mappers;
using Sitecore.Abstractions;
using Sitecore.Data.Events;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Publishing;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseLanguageManager _languageManager;
        private readonly SitecoreContentEntityModelMapper _mapper;

        public PublishingEventHandler(
            BaseLanguageManager languageManager,
            SitecoreContentEntityModelMapper mapper)
        {
            _languageManager = languageManager;
            _mapper = mapper;
        }

        public void OnItemPublished(object sender, EventArgs args)
        {
            if (args is PublishEndRemoteEventArgs remoteEventArgs)
            {
                Language language = _languageManager.GetLanguage(remoteEventArgs.LanguageName);
            }
            else
            {
                Publisher publisher = Event.ExtractParameter(args, 0) as Publisher;
                if (publisher == null)
                {
                    return;
                }

                Language language = publisher.Options.Language;
            }
        }
    }
}