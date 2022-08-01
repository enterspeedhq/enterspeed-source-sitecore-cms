using System;
using System.Net.Http;
using System.Threading.Tasks;
using Enterspeed.Source.SitecoreCms.V9.Extensions;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;
using Sitecore.Publishing;

namespace Enterspeed.Source.SitecoreCms.V9.Events
{
    public class PublishEndEventHandler
    {
        private static HttpClient client = new HttpClient();

        public PublishEndEventHandler(
           IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public void PublishEnd(object sender, EventArgs args)
        {
            var sitecoreArgs = args as Sitecore.Events.SitecoreEventArgs;
            if (sitecoreArgs == null)
            {
                return;
            }

            var publisher = sitecoreArgs.Parameters[0] as Publisher;

            var rootItem = publisher.Options.RootItem;

            var siteConfigurations = _enterspeedConfigurationService.GetConfiguration();
            foreach (EnterspeedSitecoreConfiguration configuration in siteConfigurations)
            {
                if (!configuration.IsEnabled)
                {
                    continue;
                }

                if (!HasAllowedPath(rootItem))
                {
                    continue;
                }

                EnterspeedSiteInfo siteOfItem = configuration.GetSite(rootItem);
                if (siteOfItem == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(siteOfItem.PublishHookUrl))
                {
                    continue;
                }

                var result = CallHookAsync(siteOfItem.PublishHookUrl);
            }
        }

        private static bool HasAllowedPath(Item item)
        {
            return item.IsContentItem() || item.IsRenderingItem() || item.IsDictionaryItem();
        }

        private static async Task<string> CallHookAsync(string path)
        {
            string result = null;
            HttpResponseMessage response = await client.PostAsync(path, null);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<string>();
            }

            return result;
        }
    }
}