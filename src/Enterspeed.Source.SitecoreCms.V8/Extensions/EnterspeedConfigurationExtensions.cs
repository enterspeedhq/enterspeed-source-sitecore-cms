using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Extensions
{
    public static class EnterspeedConfigurationExtensions
    {
        public static EnterspeedSitecoreConfiguration GetPublishConfiguration(this EnterspeedSitecoreConfiguration me)
        {
            if (me == null)
            {
                return null;
            }

            return new EnterspeedSitecoreConfiguration
            {
                ApiKey = me.ApiKey,
                BaseUrl = me.BaseUrl,
                ConnectionTimeout = me.ConnectionTimeout,
            };
        }

        public static EnterspeedSitecoreConfiguration GetPreviewConfiguration(this EnterspeedSitecoreConfiguration me)
        {
            if (me == null || !me.IsPreview && !string.IsNullOrEmpty(me.ApiKey))
            {
                return null;
            }

            return new EnterspeedSitecoreConfiguration
            {
                ApiKey = me.ApiKey,
                BaseUrl = me.BaseUrl,
                ConnectionTimeout = me.ConnectionTimeout,
            };
        }
    }
}