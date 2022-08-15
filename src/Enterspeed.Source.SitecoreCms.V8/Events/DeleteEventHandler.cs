using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Publishing.Pipelines.PublishItem;
using Item = Sitecore.Data.Items.Item;

namespace Enterspeed.Source.SitecoreCms.V8.Events
{
    public class DeleteEventHandler : PublishItemProcessor
    {
        private readonly Database _sourceDatabase = Database.GetDatabase("master");
        private readonly Database _targetDatabase = Database.GetDatabase("web");
        private readonly IEnterspeedSitecoreJobService _enterspeedSitecoreJobService;
        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;

        public DeleteEventHandler(
            IEnterspeedSitecoreJobService enterspeedSitecoreJobService,
            IEnterspeedConfigurationService enterspeedConfigurationService)
        {
            _enterspeedSitecoreJobService = enterspeedSitecoreJobService;
            _enterspeedConfigurationService = enterspeedConfigurationService;
        }

        public override void Process([CanBeNull] PublishItemContext context)
        {
            // Ensure all conditions are met
            if (context == null)
                return;
            if (context.Aborted)
                return;
            if (context.PublishContext == null)
                return;

            var sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);
            if (sourceItem == null)
                return;

            if (!sourceItem.Paths.IsContentItem)
                return;

            var targetFolder = _targetDatabase.GetItem(sourceItem.ID);
            if (targetFolder == null)
                return;

            // Find targetItems
            var targetItems = targetFolder.Axes.GetDescendants();
            if (targetItems != null)
            {
                var siteConfigurations = _enterspeedConfigurationService.GetConfigurations();
                foreach (var configuration in siteConfigurations)
                {
                    var languages = LanguageManager.GetLanguages(_sourceDatabase);

                    foreach (var targetItem in targetItems)
                    {
                        if (_sourceDatabase.GetItem(targetItem.ID) == null)
                        {
                            // Null in source and present in target. Delete
                            // Iterating trough language to ensure that we delete all language variants in Enterspeed
                            foreach (var language in languages)
                            {
                                Delete(targetItem, configuration, language);
                            }
                        }
                        var items = targetItem.Axes.GetDescendants();
                        if (items == null)
                            continue;
                        foreach (var item in items)
                        {
                            if (_sourceDatabase.GetItem(item.ID) == null)
                            {
                                foreach (var language in languages)
                                {
                                    Delete(item, configuration, language);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Delete(Item sourceItem, EnterspeedSitecoreConfiguration enterspeedConfiguration, Language language)
        {
            _enterspeedSitecoreJobService.HandleContentItem(sourceItem, enterspeedConfiguration, true, false, language);
            _enterspeedSitecoreJobService.HandleRendering(sourceItem, enterspeedConfiguration, true, false, language);
            _enterspeedSitecoreJobService.HandleDictionary(sourceItem, enterspeedConfiguration, true, false, language);
        }
    }
}