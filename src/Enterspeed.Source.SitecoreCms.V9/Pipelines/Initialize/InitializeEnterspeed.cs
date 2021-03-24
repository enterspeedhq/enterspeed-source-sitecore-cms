using System;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.SecurityModel;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Pipelines.Initialize
{
    public class InitializeEnterspeed
    {
        private readonly BaseItemManager _itemManager;
        private readonly BaseFactory _factory;
        private readonly BasePublishManager _publishManager;

        public InitializeEnterspeed(
            BaseItemManager itemManager,
            BaseFactory factory,
            BasePublishManager publishManager)
        {
            _itemManager = itemManager;
            _factory = factory;
            _publishManager = publishManager;
        }

        public void Process(PipelineArgs args)
        {
            Init();
        }

        private void Init()
        {
            using (new SecurityDisabler())
            {
                Database masterDb = _factory.GetDatabase("master");
                Database webDb = _factory.GetDatabase("web");
                Language language = Language.Parse("en");

                AssertOrCreateTemplates(masterDb, webDb, language);

                AssertOrCreateConfigItem(masterDb, webDb, language);
            }
        }

        private void AssertOrCreateTemplates(Database masterDb, Database webDb, Language language)
        {
            // Assert / create template item
            Item templatesSystemRoot = _itemManager.GetItem("/sitecore/templates/System", language, Version.Latest, masterDb);
            if (templatesSystemRoot == null || templatesSystemRoot.Versions.Count == 0)
            {
                throw new InvalidOperationException("Unable to find System Root (/sitecore/templates/System) in Sitecore (master database) with the language \"en\".");
            }

            Item enterspeedRootTemplatesFolder = templatesSystemRoot.Children["Enterspeed"];
            if (enterspeedRootTemplatesFolder != null)
            {
                return;
            }

            enterspeedRootTemplatesFolder = templatesSystemRoot.Add("Enterspeed", new TemplateID(TemplateIDs.TemplateFolder), EnterspeedIDs.Templates.EnterspeedFolderID);

            Item enterspeedConfigTemplateItem = enterspeedRootTemplatesFolder.Add("Configuration", new TemplateID(TemplateIDs.Template), EnterspeedIDs.Templates.EnterspeedConfigurationID);

            using (new EditContext(enterspeedConfigTemplateItem))
            {
                enterspeedConfigTemplateItem[FieldIDs.BaseTemplate] = $"{TemplateIDs.StandardTemplate}";
                enterspeedConfigTemplateItem.Appearance.Icon = "applications/32x32/gear_refresh.png";
            }

            Item enterspeedConfigSection = enterspeedConfigTemplateItem.Add("Data", new TemplateID(TemplateIDs.TemplateSection), EnterspeedIDs.Templates.EnterspeedConfigurationDataSectionID);
            Item enterspeedConfigurationApiBaseUrlField = enterspeedConfigSection.Add("API Base Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiBaseUrlFieldID);

            using (new EditContext(enterspeedConfigurationApiBaseUrlField))
            {
                enterspeedConfigurationApiBaseUrlField[TemplateFieldIDs.Type] = "Single-Line Text";
                enterspeedConfigurationApiBaseUrlField[TemplateFieldIDs.Shared] = "1";
                enterspeedConfigurationApiBaseUrlField[TemplateFieldIDs.Unversioned] = "1";
            }

            Item enterspeedConfigurationApiKeyField = enterspeedConfigSection.Add("API Key", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiKeyFieldID);

            using (new EditContext(enterspeedConfigurationApiKeyField))
            {
                enterspeedConfigurationApiKeyField[TemplateFieldIDs.Type] = "Single-Line Text";
                enterspeedConfigurationApiKeyField[TemplateFieldIDs.Shared] = "1";
                enterspeedConfigurationApiKeyField[TemplateFieldIDs.Unversioned] = "1";
                enterspeedConfigurationApiBaseUrlField.Help.ToolTip = "For example \"source-aa5c397d-bd6c-47be-a32d-34df9492d056\".";
            }

            Item enterspeedConfigurationEnabledSitesField = enterspeedConfigSection.Add("Enabled Sites", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnabledSitesFieldID);

            using (new EditContext(enterspeedConfigurationEnabledSitesField))
            {
                enterspeedConfigurationEnabledSitesField[TemplateFieldIDs.Type] = "Treelist";
                enterspeedConfigurationEnabledSitesField[TemplateFieldIDs.Source] = "/sitecore/content";
                enterspeedConfigurationEnabledSitesField[TemplateFieldIDs.Shared] = "1";
                enterspeedConfigurationEnabledSitesField[TemplateFieldIDs.Unversioned] = "1";
                enterspeedConfigurationApiBaseUrlField.Help.ToolTip = "Select the site items here with the same fullPath as the rootPath configured for the respective site(s).";
            }

            _publishManager.PublishItem(templatesSystemRoot, new[] { webDb }, new[] { language }, true, false, true);
        }

        private void AssertOrCreateConfigItem(Database masterDb, Database webDb, Language language)
        {
            // Assert / create configuration item
            Item systemRoot = _itemManager.GetItem(ItemIDs.SystemRoot, language, Version.Latest, masterDb);
            if (systemRoot == null || systemRoot.Versions.Count == 0)
            {
                throw new InvalidOperationException("Unable to find System Root (/sitecore/system) in Sitecore (master database) with the language \"en\".");
            }

            Item enterspeedChildItem = systemRoot.Children["Enterspeed Configuration"];
            if (enterspeedChildItem != null)
            {
                // Assume all is good
                return;
            }

            // create item
            systemRoot.Add("Enterspeed Configuration", new TemplateID(EnterspeedIDs.Templates.EnterspeedConfigurationID), EnterspeedIDs.Items.EnterspeedConfigurationID);

            _publishManager.PublishItem(systemRoot, new[] { webDb }, new[] { language }, true, false, true);
        }
    }
}