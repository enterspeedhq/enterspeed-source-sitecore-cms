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
        private const string GearIcon = "applications/32x32/gear_refresh.png";
        private const string EnabledSitesHelpText = "Select the site items here with the same fullPath as the rootPath configured for the respective site(s).";
        private const string ApiKeyHelpText = "For example \"source-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\".";

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

        private static Item EnsureEnterspeedTemplatesFolder(Item templatesSystemRoot)
        {
            return templatesSystemRoot.Children["Enterspeed"] ??
                templatesSystemRoot.Add("Enterspeed", new TemplateID(TemplateIDs.TemplateFolder), EnterspeedIDs.Templates.EnterspeedFolderID);
        }

        private static Item EnsureEnterspeedConfigurationTemplate(Item enterspeedRootTemplatesFolder)
        {
            return enterspeedRootTemplatesFolder.Children["Configuration"] ??
                enterspeedRootTemplatesFolder.Add("Configuration", new TemplateID(TemplateIDs.Template), EnterspeedIDs.Templates.EnterspeedConfigurationID);
        }

        private static void EnsureEnterspeedConfigurationTemplateFields(Item enterspeedConfigTemplateItem)
        {
            using (new EditContext(enterspeedConfigTemplateItem))
            {
                string currentBaseTemplate = enterspeedConfigTemplateItem[FieldIDs.BaseTemplate];
                string standardTemplate = TemplateIDs.StandardTemplate.ToString();

                if (!currentBaseTemplate.Equals(standardTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    enterspeedConfigTemplateItem[FieldIDs.BaseTemplate] = standardTemplate;
                }

                if (!enterspeedConfigTemplateItem.Appearance.Icon.Equals(GearIcon, StringComparison.OrdinalIgnoreCase))
                {
                    enterspeedConfigTemplateItem.Appearance.Icon = GearIcon;
                }
            }
        }

        private static Item EnsureEnterspeedConfigurationTemplateDataSection(Item enterspeedConfigTemplateItem)
        {
            return enterspeedConfigTemplateItem.Children["Data"]
                ?? enterspeedConfigTemplateItem.Add("Data", new TemplateID(TemplateIDs.TemplateSection), EnterspeedIDs.Templates.EnterspeedConfigurationDataSectionID);
        }

        private static void EnsureEnterspeedConfigurationDataSectionFields(Item enterspeedConfigSection)
        {
            Item apiBaseUrlField = enterspeedConfigSection.Children["API Base Url"]
                ?? enterspeedConfigSection.Add("API Base Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiBaseUrlFieldID);

            using (new EditContext(apiBaseUrlField))
            {
                string currentTypeValue = apiBaseUrlField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Single-Line Text", StringComparison.OrdinalIgnoreCase))
                {
                    apiBaseUrlField[TemplateFieldIDs.Type] = "Single-Line Text";
                }

                string currentSharedValue = apiBaseUrlField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "1")
                {
                    apiBaseUrlField[TemplateFieldIDs.Shared] = "1";
                }

                string currentUnversionedValue = apiBaseUrlField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    apiBaseUrlField[TemplateFieldIDs.Unversioned] = "1";
                }
            }

            Item apiKeyField = enterspeedConfigSection.Children["API Key"]
                ?? enterspeedConfigSection.Add("API Key", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiKeyFieldID);

            using (new EditContext(apiKeyField))
            {
                string currentTypeValue = apiKeyField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Single-Line Text", StringComparison.OrdinalIgnoreCase))
                {
                    apiKeyField[TemplateFieldIDs.Type] = "Single-Line Text";
                }

                string currentSharedValue = apiKeyField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "1")
                {
                    apiKeyField[TemplateFieldIDs.Shared] = "1";
                }

                string currentUnversionedValue = apiKeyField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    apiKeyField[TemplateFieldIDs.Unversioned] = "1";
                }

                string currentHelpValue = apiKeyField.Help.ToolTip;
                if (!currentHelpValue.Equals(ApiKeyHelpText, StringComparison.OrdinalIgnoreCase))
                {
                    apiKeyField.Help.ToolTip = ApiKeyHelpText;
                }
            }

            Item enabledSitesField = enterspeedConfigSection.Children["Enabled Sites"]
                ?? enterspeedConfigSection.Add("Enabled Sites", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnabledSitesFieldID);

            using (new EditContext(enabledSitesField))
            {
                string currentTypeValue = enabledSitesField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Treelist", StringComparison.OrdinalIgnoreCase))
                {
                    enabledSitesField[TemplateFieldIDs.Type] = "Treelist";
                }

                enabledSitesField[TemplateFieldIDs.Source] = "/sitecore/content";

                string currentSharedValue = enabledSitesField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "1")
                {
                    enabledSitesField[TemplateFieldIDs.Shared] = "1";
                }

                string currentUnversionedValue = enabledSitesField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    enabledSitesField[TemplateFieldIDs.Unversioned] = "1";
                }

                string currentHelpValue = enabledSitesField.Help.ToolTip;
                if (!currentHelpValue.Equals(EnabledSitesHelpText, StringComparison.OrdinalIgnoreCase))
                {
                    enabledSitesField.Help.ToolTip = EnabledSitesHelpText;
                }
            }
        }

        private static void EnsureEnterspeedConfigurationItem(Item systemRoot)
        {
            if (systemRoot.Children["Enterspeed Configuration"] == null)
            {
                systemRoot.Add("Enterspeed Configuration", new TemplateID(EnterspeedIDs.Templates.EnterspeedConfigurationID), EnterspeedIDs.Items.EnterspeedConfigurationID);
            }
        }

        private void Init()
        {
            using (new SecurityDisabler())
            {
                Database masterDb = _factory.GetDatabase("master");
                Database webDb = _factory.GetDatabase("web");
                Language language = Language.Parse("en");

                EnsureTemplates(masterDb, webDb, language);

                EnsureItems(masterDb, webDb, language);
            }
        }

        private void EnsureTemplates(Database masterDb, Database webDb, Language language)
        {
            // Assert / create template item
            Item templatesSystemRoot = _itemManager.GetItem("/sitecore/templates/System", language, Version.Latest, masterDb);
            if (templatesSystemRoot == null || templatesSystemRoot.Versions.Count == 0)
            {
                throw new InvalidOperationException("Unable to find System Root (/sitecore/templates/System) in Sitecore (master database) with the language \"en\".");
            }

            Item enterspeedConfigTemplateItem = EnsureEnterspeedConfigurationTemplate(EnsureEnterspeedTemplatesFolder(templatesSystemRoot));

            EnsureEnterspeedConfigurationTemplateFields(enterspeedConfigTemplateItem);

            Item enterspeedConfigSection = EnsureEnterspeedConfigurationTemplateDataSection(enterspeedConfigTemplateItem);

            EnsureEnterspeedConfigurationDataSectionFields(enterspeedConfigSection);

            _publishManager.PublishItem(templatesSystemRoot, new[] { webDb }, new[] { language }, true, false, true);
        }

        private void EnsureItems(Database masterDb, Database webDb, Language language)
        {
            // Assert / create configuration item
            Item systemRoot = _itemManager.GetItem(ItemIDs.SystemRoot, language, Version.Latest, masterDb);
            if (systemRoot == null || systemRoot.Versions.Count == 0)
            {
                throw new InvalidOperationException("Unable to find System Root (/sitecore/system) in Sitecore (master database) with the language \"en\".");
            }

            EnsureEnterspeedConfigurationItem(systemRoot);

            _publishManager.PublishItem(systemRoot, new[] { webDb }, new[] { language }, true, false, true);
        }
    }
}