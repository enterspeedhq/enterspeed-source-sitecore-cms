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
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Controllers
{
    public class EnterspeedController : Controller
    {
        public EnterspeedController()
        {
         
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}