using System.Linq;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;

namespace Enterspeed.Source.SitecoreCms.V8.Providers
{
    public class EnterspeedSitecoreConfigurationProvider : IEnterspeedConfigurationProvider
    {
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public EnterspeedSitecoreConfigurationProvider(
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public EnterspeedConfiguration Configuration => _enterspeedConfigurationService.GetConfigurations().Any() ? _enterspeedConfigurationService.GetConfigurations().First() : new EnterspeedConfiguration();
    }
}