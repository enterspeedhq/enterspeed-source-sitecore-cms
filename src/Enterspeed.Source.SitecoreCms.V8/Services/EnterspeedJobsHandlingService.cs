using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Enterspeed.Source.SitecoreCms.V8.Data.Repositories;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedJobsHandlingService : IEnterspeedJobsHandlingService
    {
        private readonly IEnterspeedJobRepository _enterspeedJobRepository;
        private readonly IEnterspeedJobsHandler _enterspeedJobsHandler;
        private readonly IEnterspeedSitecoreLoggingService _enterspeedSitecoreLoggingService;

    }
}