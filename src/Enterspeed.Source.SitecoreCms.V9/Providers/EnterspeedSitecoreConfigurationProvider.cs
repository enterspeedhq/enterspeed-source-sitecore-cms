using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services;

namespace Enterspeed.Source.SitecoreCms.V9.Providers
{
    public class EnterspeedSitecoreConfigurationProvider : IEnterspeedConfigurationProvider
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public EnterspeedSitecoreConfigurationProvider(
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public EnterspeedConfiguration Configuration => _enterspeedConfigurationService.GetConfiguration().Any() ? _enterspeedConfigurationService.GetConfiguration().First() : new EnterspeedConfiguration();
    }
}