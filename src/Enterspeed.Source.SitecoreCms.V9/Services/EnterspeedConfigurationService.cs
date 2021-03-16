using Enterspeed.Source.SitecoreCms.V9.Exceptions;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore.Abstractions;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedConfigurationService : IEnterspeedConfigurationService
    {
        private readonly BaseSettings _settings;
        private EnterspeedSitecoreConfiguration _configuration;

        public EnterspeedConfigurationService(
            BaseSettings settings)
        {
            _settings = settings;
        }

        public EnterspeedSitecoreConfiguration GetConfiguration()
        {
            if (_configuration != null)
            {
                return _configuration;
            }

            _configuration = new EnterspeedSitecoreConfiguration
            {
                BaseUrl = GetEnterspeedBaseUrl(_settings),
                ApiKey = GetEnterspeedApiKey(_settings)
            };

            return _configuration;
        }

        private static string GetEnterspeedBaseUrl(BaseSettings settings)
        {
            string baseUrl = settings.GetSetting("Enterspeed.BaseUrl", null);
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed Base Url from the Sitecore Setting \"Enterspeed.BaseUrl\".");
            }

            return baseUrl;
        }

        private static string GetEnterspeedApiKey(BaseSettings settings)
        {
            string apiKey = settings.GetSetting("Enterspeed.ApiKey", null);
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new EnterspeedSitecoreException(
                    "Unable to retrieve Enterspeed API Key from the Sitecore Setting \"Enterspeed.ApiKey\".");
            }

            return apiKey;
        }
    }
}