using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.SitecoreCms.V8.Guards;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedGuardService : IEnterspeedGuardService
    {
        private readonly IEnterspeedSitecoreLoggingService _loggingService;
        private readonly List<IEnterspeedItemHandlingGuard> _itemHandlingGuards;

        public EnterspeedGuardService(
            IEnterspeedSitecoreLoggingService loggingService,
            List<IEnterspeedItemHandlingGuard> itemHandlingGuards)
        {
            _loggingService = loggingService;
            _itemHandlingGuards = itemHandlingGuards;
        }

        //TODO: Implement guards
        public bool CanIngest(Item content, string culture)
        {
            var blockingGuard = _itemHandlingGuards.FirstOrDefault(guard => !guard.CanIngest(content, culture));
            if (blockingGuard == null)
            {
                return true;
            }

            _loggingService.Debug($@"Item {content.ID} with {culture} culture, ingest avoided by '{blockingGuard.GetType().Name}'.");
            return false;
        }
    }
}