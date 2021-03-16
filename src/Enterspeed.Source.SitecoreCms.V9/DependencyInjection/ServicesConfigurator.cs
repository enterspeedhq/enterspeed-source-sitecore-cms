using Enterspeed.Source.Sdk.Api.Connection;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.SitecoreCms.V9.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Enterspeed.Source.SitecoreCms.V9.DependencyInjection
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<SitecoreContentEntityModelMapper>();
            services.AddScoped<IContentIdentityService, SitecoreContentIdentityService>();
            services.AddTransient<IEnterspeedPropertyService, EnterspeedPropertyService>();
            services.AddSingleton<IEnterspeedIngestService, EnterspeedIngestService>();
            services.AddSingleton<IEnterspeedConfigurationService, EnterspeedConfigurationService>();
            services.AddSingleton<IEnterspeedConfigurationProvider, EnterspeedSitecoreConfigurationProvider>();

            services.AddSingleton<IEnterspeedConnection>(provider =>
            {
                var configurationProvider = provider.GetService<IEnterspeedConfigurationProvider>();
                return new EnterspeedConnection(configurationProvider);
            });
        }
    }
}