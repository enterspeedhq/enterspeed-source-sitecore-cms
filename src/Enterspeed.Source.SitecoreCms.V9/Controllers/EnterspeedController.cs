using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Mvc.Controllers;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Controllers
{
    public class EnterspeedController : SitecoreController
    {
        private readonly BaseItemManager _itemManager;
        private readonly IEntityModelMapper<Item, SitecoreContentEntity> _itemMapper;
        private readonly IJsonSerializer _jsonSerializer;

        private readonly Database _webDatabase;
        private readonly List<Language> _allLanguages;

        public EnterspeedController(
            BaseItemManager itemManager,
            BaseLanguageManager languageManager,
            BaseFactory factory,
            IEntityModelMapper<Item, SitecoreContentEntity> itemMapper,
            IJsonSerializer jsonSerializer)
        {
            _itemManager = itemManager;
            _itemMapper = itemMapper;
            _jsonSerializer = jsonSerializer;

            _webDatabase = factory.GetDatabase("web");
            _allLanguages = languageManager.GetLanguages(_webDatabase).ToList();

            var user = Sitecore.Context.User;
        }

        [HttpGet]
        public ActionResult Debug(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var items = new List<Item>();

            foreach (var language in _allLanguages)
            {
                var item = _itemManager.GetItem(id, language, Version.Latest, _webDatabase);
                if (item == null || item.Versions.Count == 0)
                {
                    continue;
                }

                items.Add(item);
            }

            return Content(
                _jsonSerializer.Serialize(
                    items.Select(
                        _itemMapper.Map)));
        }

        private static void CheckAccessRights()
        {
            if (Sitecore.Context.User.IsAuthenticated == false ||
                Sitecore.Context.User.IsAdministrator == false)
            {
                throw new UnauthorizedAccessException();
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            CheckAccessRights();
        }
    }
}