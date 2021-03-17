using System;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
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
        private readonly BaseLog _log;
        private readonly SitecoreContentEntityModelMapper _mapper;
        private readonly IContentIdentityService _identityService;
        private readonly IEnterspeedIngestService _enterspeedIngestService;

        public PublishingEventHandler(
            BaseItemManager itemManager,
            BaseLog log,
            SitecoreContentEntityModelMapper mapper,
            IContentIdentityService identityService,
            IEnterspeedIngestService enterspeedIngestService)
        {
            _itemManager = itemManager;
            _log = log;
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

                Response deleteResponse = _enterspeedIngestService.Delete(id);

                if (IsSuccessStatusCode(deleteResponse.StatusCode) == false)
                {
                    _log.Error(deleteResponse.Message ?? deleteResponse.Exception.Message, deleteResponse.Exception, this);
                }
                else
                {
                    _log.Info(deleteResponse.Message, this);
                }

                return;
            }

            Response saveResponse = _enterspeedIngestService.Save(sitecoreContentEntity);

            if (IsSuccessStatusCode(saveResponse.StatusCode) == false)
            {
                _log.Error(saveResponse.Message ?? saveResponse.Exception.Message, saveResponse.Exception, this);
            }
            else
            {
                _log.Info(saveResponse.Message, this);
            }
        }

        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}