using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V8.Factories;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedJobsHandler : IEnterspeedJobsHandler
    {
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedSitecoreLoggingService _enterspeedSitecoreLoggingService;
        private readonly IEnterspeedJobFactory _enterspeedJobFactory;

        public EnterspeedJobsHandler(IEnterspeedJobFactory enterspeedJobFactory, IEnterspeedSitecoreLoggingService enterspeedSitecoreLoggingService, IEnterspeedJobRepository enterspeedJobRepository)
        {
            _enterspeedJobFactory = enterspeedJobFactory;
            _enterspeedSitecoreLoggingService = enterspeedSitecoreLoggingService;
            _enterspeedJobRepository = enterspeedJobRepository;
        }


    }
}