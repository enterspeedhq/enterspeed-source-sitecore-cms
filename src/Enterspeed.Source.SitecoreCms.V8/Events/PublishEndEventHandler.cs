using System;
using System.Net.Http;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Publishing;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class PublishEndEventHandler
    {
        private static readonly HttpClient Client = new HttpClient();

        public PublishEndEventHandler(
           IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public void PublishEnd(object sender, EventArgs args)
        {
            if (!(args is SitecoreEventArgs sitecoreArgs))
            {
                return;
            }

            var publisher = sitecoreArgs.Parameters[0] as Publisher;
            var rootItem = publisher.Options.RootItem;

            var siteConfigurations = _enterspeedConfigurationService.GetConfigurations();
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

                // TODO: Task with no await? Any consequences to this?
                //var result = CallHookAsync(siteOfItem.PublishHookUrl);
            }
        }

        private static bool HasAllowedPath(Item item)
        {
            return item.IsContentItem() || item.IsRenderingItem() || item.IsDictionaryItem();
        }

        //private static async Task<string> CallHookAsync(string path)
        //{
        //    string result = null;
        //    HttpResponseMessage response = await Client.PostAsync(path, null);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        result = await response.Content.ReadAsStringAsync();
        //    }

        //    return result;
        //}
    }
}