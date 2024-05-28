﻿using System;
using Enterspeed.Source.SitecoreCms.V9.Services;
using Enterspeed.Source.SitecoreCms.V9.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace Enterspeed.Source.SitecoreCms.V9.Commands
{
    public class SeedEnterspeed : Command
    {
        public override void Execute(CommandContext context)
        {
            var serviceProvider = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider;

            try
            {
                var enterspeedSitecoreJobService = serviceProvider.GetService<IEnterspeedSitecoreJobService>();

                Assert.ArgumentNotNull(context, nameof(context));
                foreach (var item in context.Items)
                {
                    enterspeedSitecoreJobService.Seed(item);
                }
            }
            catch (Exception e)
            {
                var enterspeedLogger = serviceProvider.GetService<IEnterspeedSitecoreLoggingService>();
                enterspeedLogger.Error("Something went wrong when seeding data to Enterspeed", e);
                SheerResponse.Alert("Something went wrong when seeding data to Enterspeed", true);
                return;
            }

            Context.ClientPage.Start(this, "Run");
        }

        protected void Run(ClientPipelineArgs args)
        {
            SheerResponse.Alert("Successfully seeded data to Enterspeed", true);
            args.WaitForPostBack(false);
        }
    }
}
