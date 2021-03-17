using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V9.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Sitecore.Abstractions;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseLanguageManager _languageManager;
        private readonly BaseItemManager _itemManager;
        private readonly SitecoreContentEntityModelMapper _mapper;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public PublishingEventHandler(
            BaseLanguageManager languageManager,
            BaseItemManager itemManager,
            SitecoreContentEntityModelMapper mapper,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _languageManager = languageManager;
            _itemManager = itemManager;
            _mapper = mapper;
            _enterspeedIngestService = enterspeedIngestService;
        }

        public void OnComplete(object sender, EventArgs args)
        {
            var publishOptions =
                Event.ExtractParameter<IEnumerable<DistributedPublishOptions>>(args, 0)?.ToList() ??
                new List<DistributedPublishOptions>();
            if (publishOptions.Any() == false)
            {
                return;
            }

            foreach (var publishOption in publishOptions)
            {
                // For whatever reason, if the target database is not web, we skip this
                if (publishOption.TargetDatabaseName.Equals("web", StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                string culture = publishOption.LanguageName;
            }
        }

        public void OnItemProcessed(object sender, EventArgs args)
        {
            PublishItemContext context = args is ItemProcessedEventArgs itemProcessedEventArgs
                ? itemProcessedEventArgs.Context
                : null;

            if (context == null)
            {
                return;
            }

            if (context.Action != PublishAction.DeleteTargetItem &&
                context.Action != PublishAction.PublishVersion)
            {
                return;
            }

            Language language = context.PublishOptions.Language;

            Item item = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.TargetDatabase);
            if (item == null || item.Versions.Count == 0)
            {
                return;
            }

            SitecoreContentEntity sitecoreContentEntity = _mapper.Map(item);

            _enterspeedIngestService.Save(sitecoreContentEntity);

            // TODO
        }
    }
}