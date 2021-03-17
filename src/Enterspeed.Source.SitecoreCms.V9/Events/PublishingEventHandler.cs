using System;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishingEventHandler
    {
        private readonly BaseItemManager _itemManager;
        private readonly SitecoreContentEntityModelMapper _mapper;
        private readonly IContentIdentityService _identityService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            SitecoreContentEntityModelMapper mapper,
            IContentIdentityService identityService,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _itemManager = itemManager;
            _mapper = mapper;
            _identityService = identityService;
            _enterspeedIngestService = enterspeedIngestService;
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

            var itemIsDeleted = context.Action == PublishAction.DeleteTargetItem;
            var itemIsPublished = context.Action == PublishAction.PublishVersion;

            if (itemIsDeleted == false && itemIsPublished == false)
            {
                return;
            }

            Language language = context.PublishOptions.Language;

            Item item = _itemManager.GetItem(context.ItemId, language, Version.Latest, context.PublishHelper.Options.TargetDatabase);
            if (item == null || item.Versions.Count == 0)
            {
                return;
            }

            // Skip, if the item published is not a content item
            if (item.Paths.FullPath.StartsWith("/sitecore/content", StringComparison.OrdinalIgnoreCase) == false)
            {
                return;
            }

            SitecoreContentEntity sitecoreContentEntity = _mapper.Map(item);
            if (sitecoreContentEntity == null)
            {
                return;
            }

            if (itemIsDeleted)
            {
                string id = _identityService.GetId(context.ItemId.Guid, language);

                _enterspeedIngestService.Delete(id);

                return;
            }

            _enterspeedIngestService.Save(sitecoreContentEntity);

            // TODO
        }
    }
}