using Enterspeed.Source.Sdk.Api.Connection;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.Sdk.Domain.SystemTextJson;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Enterspeed.Source.SitecoreCms.V9.DependencyInjection
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IEnterspeedPropertyService, EnterspeedPropertyService>();
            services.AddSingleton<IContentIdentityService, SitecoreContentIdentityService>();
            services.AddSingleton<SitecoreContentEntityModelMapper>();
            services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
            services.AddSingleton<IEnterspeedIngestService, EnterspeedIngestService>();
            services.AddSingleton<IEnterspeedConfigurationService, EnterspeedConfigurationService>();
            services.AddSingleton<IEnterspeedConfigurationProvider, EnterspeedSitecoreConfigurationProvider>();
            services.AddSingleton<IEnterspeedUrlService, EnterspeedSitecoreUrlService>();
            services.AddSingleton<EnterspeedDateFormatter>();

            services.AddSingleton<IEnterspeedConnection>(provider =>
            {
                var configurationProvider = provider.GetService<IEnterspeedConfigurationProvider>();
                return new EnterspeedConnection(configurationProvider);
            });

            RegisterFieldConverters(services);
        }

        private static void RegisterFieldConverters(IServiceCollection services)
        {
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultSingleLineTextFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultRichTextFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultCheckboxFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultDateFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultFileFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultImageFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultIntegerFieldValueConverter>();
        }
    }
}