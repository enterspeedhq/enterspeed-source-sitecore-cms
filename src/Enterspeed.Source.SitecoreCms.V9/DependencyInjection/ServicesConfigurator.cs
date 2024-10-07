﻿using Enterspeed.Source.Sdk.Api.Connection;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Api.Services;
using Enterspeed.Source.Sdk.Domain.Connection;
using Enterspeed.Source.Sdk.Domain.Services;
using Enterspeed.Source.Sdk.Domain.SystemTextJson;
using Enterspeed.Source.SitecoreCms.V9.Controllers;
using Enterspeed.Source.SitecoreCms.V9.Data;
using Enterspeed.Source.SitecoreCms.V9.Data.Repositories;
using Enterspeed.Source.SitecoreCms.V9.Factories;
using Enterspeed.Source.SitecoreCms.V9.Handlers;
using Enterspeed.Source.SitecoreCms.V9.Handlers.Content;
using Enterspeed.Source.SitecoreCms.V9.Handlers.Dictionaries;
using Enterspeed.Source.SitecoreCms.V9.Handlers.PreviewContent;
using Enterspeed.Source.SitecoreCms.V9.Handlers.Rendering;
using Enterspeed.Source.SitecoreCms.V9.Models;
using Enterspeed.Source.SitecoreCms.V9.Models.Mappers;
using Enterspeed.Source.SitecoreCms.V9.Providers;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.DefaultFieldConverters;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;

namespace Enterspeed.Source.SitecoreCms.V9.DependencyInjection
{
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IEnterspeedSitecoreLoggingService, EnterspeedSitecoreLoggingService>();
            services.AddSingleton<IEnterspeedFieldConverter, EnterspeedFieldConverter>();
            services.AddSingleton<IEnterspeedPropertyService, EnterspeedPropertyService>();
            services.AddSingleton<IEnterspeedSitecoreFieldService, EnterspeedSitecoreFieldService>();
            services.AddSingleton<IEnterspeedIdentityService, EnterspeedSitecoreIdentityService>();
            services.AddSingleton<IEntityModelMapper<Item, SitecoreContentEntity>, SitecoreContentEntityModelMapper>();
            services.AddSingleton<IEntityModelMapper<RenderingItem, SitecoreRenderingEntity>, SitecoreRenderingEntityModelMapper>();
            services.AddSingleton<IEntityModelMapper<Item, SitecoreDictionaryEntity>, SitecoreDictionaryEntityModelMapper>();
            services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
            services.AddSingleton<IEnterspeedIngestService, EnterspeedIngestService>();
            services.AddSingleton<IEnterspeedConfigurationService, EnterspeedConfigurationService>();
            services.AddSingleton<IEnterspeedConfigurationProvider, EnterspeedSitecoreConfigurationProvider>();
            services.AddSingleton<IEnterspeedUrlService, EnterspeedSitecoreUrlService>();
            services.AddSingleton<IEnterspeedSitecoreIngestService, EnterspeedSitecoreIngestService>();
            services.AddTransient<IEnterspeedSitecoreJobService, EnterspeedSitecoreJobService>();
            services.AddSingleton<EnterspeedDateFormatter>();

            services.AddSingleton<IEnterspeedConnection>(provider =>
            {
                var configurationProvider = provider.GetService<IEnterspeedConfigurationProvider>();
                return new EnterspeedConnection(configurationProvider);
            });

            RegisterFieldConverters(services);
            RegisterQueueImplementations(services);
            RegisterControllers(services);
        }

        private static void RegisterQueueImplementations(IServiceCollection services)
        {
            services.AddTransient<IEnterspeedMigrationService, EnterspeedMigrationService>();
            services.AddTransient<IEnterspeedJobsHandlingService, EnterspeedJobsHandlingService>();
            services.AddTransient<IEnterspeedJobRepository, EnterspeedJobRepository>();
            services.AddTransient<IEnterspeedJobFactory, EnterspeedJobFactory>();
            services.AddTransient<IEnterspeedGuardService, EnterspeedGuardService>();

            // Job handlers
            services.AddTransient<IEnterspeedJobsHandler, EnterspeedJobsHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedRenderingDeleteJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedRenderingPublishJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedContentDeleteJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedContentPublishJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedPreviewContentDeleteJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedPreviewContentPublishJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedDictionaryItemDeleteJobHandler>();
            services.AddTransient<IEnterspeedJobHandler, EnterspeedDictionaryItemPublishJobHandler>();
        }

        private static void RegisterFieldConverters(IServiceCollection services)
        {
            // Simple Types
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultSingleLineTextFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultRichTextFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultCheckboxFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultDateFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultFileFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultImageFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultIntegerFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultMultiLineTextFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultNumberFieldValueConverter>();

            // List Types
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultChecklistFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultDroplistFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultGroupedDroplinkFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultGroupedDroplistFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultMultilistFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultNameValueListFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultNameLookupValueListFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultTreelistFieldValueConverter>();

            // Link Types
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultDroplinkFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultDroptreeFieldValueConverter>();
            services.AddSingleton<IEnterspeedFieldValueConverter, DefaultGeneralLinkFieldValueConverter>();
        }

        private static void RegisterControllers(IServiceCollection services)
        {
            services.AddTransient<EnterspeedController>();
        }
    }
}