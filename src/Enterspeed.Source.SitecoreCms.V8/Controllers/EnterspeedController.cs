using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V8.Services;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Mvc.Controllers;
using Version = Sitecore.Data.Version;


namespace Enterspeed.Source.SitecoreCms.V8.Controllers
{
    public class EnterspeedController : SitecoreController
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly BaseItemManager _itemManager;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _itemMapper;
        private readonly IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> _renderingMapper;
        private readonly IEntityModelMapper<Item, SitecoreDictionaryEntity> _dictionaryMapper;
        private readonly Enterspeed.Source.Sdk.Api.Services.IJsonSerializer _jsonSerializer;

        private readonly Database _webDatabase;
        private readonly List<Language> _allLanguages;

        public EnterspeedController(
            IEnterspeedIdentityService enterspeedIdentityService,
            BaseItemManager itemManager,
            BaseLanguageManager languageManager,
            BaseFactory factory,
            IEntityModelMapper<Item, SitecoreContentEntity> itemMapper,
            IEntityModelMapper<RenderingItem, SitecoreRenderingEntity> renderingMapper,
            IEntityModelMapper<Item, SitecoreDictionaryEntity> dictionaryMapper,
            IJsonSerializer jsonSerializer)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
            _itemManager = itemManager;
            _itemMapper = itemMapper;
            _renderingMapper = renderingMapper;
            _dictionaryMapper = dictionaryMapper;
            _jsonSerializer = jsonSerializer;

            _webDatabase = factory.GetDatabase("web");
            _allLanguages = languageManager.GetLanguages(_webDatabase).ToList();
        }

        [HttpGet]
        public ActionResult Debug(string id, EnterspeedSitecoreConfiguration configuration)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            bool isPath = id.StartsWith("/sitecore", StringComparison.OrdinalIgnoreCase);
            if (isPath)
            {
                var items = new List<Item>();

                foreach (var language in _allLanguages)
                {
                    Item itemOfPath = _itemManager.GetItem(id, language, Version.Latest, _webDatabase);
                    if (itemOfPath == null || itemOfPath.Versions.Count == 0)
                    {
                        continue;
                    }

                    items.Add(itemOfPath);
                }

                var entities = new List<IEnterspeedEntity>();

                foreach (Item item in items)
                {
                    entities.Add(Map(item, configuration));
                }

                return Content(_jsonSerializer.Serialize(entities));
            }

            EnterspeedSitecoreIdentity identity = _enterspeedIdentityService.Parse(id);
            if (identity == null)
            {
                return null;
            }

            Item itemOfId = _itemManager.GetItem(identity.ID, identity.Language, Version.Latest, _webDatabase);
            if (itemOfId == null || itemOfId.Versions.Count == 0)
            {
                return null;
            }

            return Content(_jsonSerializer.Serialize(Map(itemOfId, configuration)));
        }

        private static void CheckAccessRights()
        {
            if (!Sitecore.Context.User.IsAuthenticated ||
                !Sitecore.Context.User.IsAdministrator)
            {
                throw new UnauthorizedAccessException();
            }
        }

        private IEnterspeedEntity Map(Item item, EnterspeedSitecoreConfiguration configuration)
        {
            IEnterspeedEntity entity;

            if (item.IsRenderingItem())
            {
                entity = _renderingMapper.Map(item, configuration);
            }
            else if (item.IsDictionaryItem())
            {
                entity = _dictionaryMapper.Map(item, configuration);
            }
            else
            {
                entity = _itemMapper.Map(item, configuration);
            }

            return entity;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            CheckAccessRights();
        }
    }
}