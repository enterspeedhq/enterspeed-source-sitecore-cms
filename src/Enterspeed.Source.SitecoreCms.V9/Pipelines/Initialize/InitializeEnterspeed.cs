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
        private const string SiteIcon = "Applications/32x32/window_gear.png";
        private const string EnabledSitesHelpText = "Select the site items here with the same fullPath as the rootPath configured for the respective site(s).";
        private const string ApiKeyHelpText = "For example \"source-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\".";
        private const string EnabledDictionariesHelpText = "Select the dictionary parent item, to push the item and all descendant dictionaries to Enterspeed.";

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
            Item enabledField = enterspeedConfigSection.Children["Enabled"]
                ?? enterspeedConfigSection.Add("Enabled", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnabledFieldID);

            using (new EditContext(enabledField))
            {
                enabledField.Appearance.Sortorder = 100;

                string currentTypeValue = enabledField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Checkbox", StringComparison.OrdinalIgnoreCase))
                {
                    enabledField[TemplateFieldIDs.Type] = "Checkbox";
                }

                string currentSharedValue = enabledField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "1")
                {
                    enabledField[TemplateFieldIDs.Shared] = "1";
                }

                string currentUnversionedValue = enabledField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    enabledField[TemplateFieldIDs.Unversioned] = "1";
                }
            }
        }

        private static Item EnsureEnterspeedSiteConfigurationTemplate(Item enterspeedRootTemplatesFolder)
        {
            return enterspeedRootTemplatesFolder.Children["Site Configuration"] ??
                enterspeedRootTemplatesFolder.Add("Site Configuration", new TemplateID(TemplateIDs.Template), EnterspeedIDs.Templates.EnterspeedSiteConfigurationID);
        }

        private static void EnsureEnterspeedConfigurationItem(Item systemRoot, out Item newEnterspeedConfigurationItem)
        {
            newEnterspeedConfigurationItem = null;

            if (systemRoot.Children["Enterspeed Configuration"] == null)
            {
                var settingsItem = systemRoot.Add("Enterspeed Configuration", new TemplateID(EnterspeedIDs.Templates.EnterspeedConfigurationID), EnterspeedIDs.Items.EnterspeedConfigurationID);
                settingsItem.Editing.BeginEdit();
                settingsItem.Fields["__Masters"].Value = EnterspeedIDs.Templates.EnterspeedSiteConfigurationID.ToString();
                settingsItem.Editing.EndEdit();

                newEnterspeedConfigurationItem = settingsItem;
            }
        }

        private static void EnsureEnterspeedSiteConfigurationTemplateFields(Item enterspeedSiteConfigTemplateItem)
        {
            using (new EditContext(enterspeedSiteConfigTemplateItem))
            {
                string currentBaseTemplate = enterspeedSiteConfigTemplateItem[FieldIDs.BaseTemplate];
                string standardTemplate = TemplateIDs.StandardTemplate.ToString();

                if (!currentBaseTemplate.Equals(standardTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    enterspeedSiteConfigTemplateItem[FieldIDs.BaseTemplate] = standardTemplate;
                }

                if (!enterspeedSiteConfigTemplateItem.Appearance.Icon.Equals(SiteIcon, StringComparison.OrdinalIgnoreCase))
                {
                    enterspeedSiteConfigTemplateItem.Appearance.Icon = SiteIcon;
                }
            }
        }

        private static Item EnsureEnterspeedSiteConfigurationTemplateDataSection(Item enterspeedSiteConfigTemplateItem)
        {
            return enterspeedSiteConfigTemplateItem.Children["Data"]
                ?? enterspeedSiteConfigTemplateItem.Add("Data", new TemplateID(TemplateIDs.TemplateSection), EnterspeedIDs.Templates.EnterspeedSiteConfigurationDataSectionID);
        }

        private static void EnsureEnterspeedSiteConfigurationDataSectionFields(Item enterspeedSiteConfigSection)
        {
            Item apiBaseUrlField = enterspeedSiteConfigSection.Children["API Base Url"]
                ?? enterspeedSiteConfigSection.Add("API Base Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiBaseUrlFieldID);

            using (new EditContext(apiBaseUrlField))
            {
                apiBaseUrlField.Appearance.Sortorder = 110;

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

            Item apiKeyField = enterspeedSiteConfigSection.Children["API Key"]
                ?? enterspeedSiteConfigSection.Add("API Key", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedApiKeyFieldID);

            using (new EditContext(apiKeyField))
            {
                apiKeyField.Appearance.Sortorder = 120;

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

            Item enabledSitesField = enterspeedSiteConfigSection.Children["Enabled Sites"]
                ?? enterspeedSiteConfigSection.Add("Enabled Sites", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnabledSitesFieldID);

            using (new EditContext(enabledSitesField))
            {
                enabledSitesField.Appearance.Sortorder = 130;

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

            Item dictionariesField = enterspeedSiteConfigSection.Children["Enabled Dictionaries"]
                                     ?? enterspeedSiteConfigSection.Add("Enabled Dictionaries", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnabledDictionariesFieldID);

            using (new EditContext(dictionariesField))
            {
                dictionariesField.Appearance.Sortorder = 135;

                string currentTypeValue = dictionariesField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Treelist", StringComparison.OrdinalIgnoreCase))
                {
                    dictionariesField[TemplateFieldIDs.Type] = "Treelist";
                }

                dictionariesField[TemplateFieldIDs.Source] = "/sitecore/System/Dictionary";

                string currentSharedValue = dictionariesField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "1")
                {
                    dictionariesField[TemplateFieldIDs.Shared] = "1";
                }

                string currentUnversionedValue = dictionariesField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    dictionariesField[TemplateFieldIDs.Unversioned] = "1";
                }

                string currentHelpValue = dictionariesField.Help.ToolTip;
                if (!currentHelpValue.Equals(EnabledSitesHelpText, StringComparison.OrdinalIgnoreCase))
                {
                    dictionariesField.Help.ToolTip = EnabledDictionariesHelpText;
                }
            }

            Item siteBaseUrlField = enterspeedSiteConfigSection.Children["Site Base Url"]
                ?? enterspeedSiteConfigSection.Add("Site Base Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedSiteBaseUrlFieldID);

            using (new EditContext(siteBaseUrlField))
            {
                siteBaseUrlField.Appearance.Sortorder = 140;

                string currentTypeValue = siteBaseUrlField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Single-Line Text", StringComparison.OrdinalIgnoreCase))
                {
                    siteBaseUrlField[TemplateFieldIDs.Type] = "Single-Line Text";
                }

                string currentSharedValue = siteBaseUrlField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "0")
                {
                    siteBaseUrlField[TemplateFieldIDs.Shared] = "0";
                }

                string currentUnversionedValue = siteBaseUrlField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    siteBaseUrlField[TemplateFieldIDs.Unversioned] = "1";
                }
            }

            Item mediaBaseUrlField = enterspeedSiteConfigSection.Children["Media Base Url"]
                ?? enterspeedSiteConfigSection.Add("Media Base Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedMediaBaseUrlFieldID);

            using (new EditContext(mediaBaseUrlField))
            {
                mediaBaseUrlField.Appearance.Sortorder = 140;

                string currentTypeValue = mediaBaseUrlField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Single-Line Text", StringComparison.OrdinalIgnoreCase))
                {
                    mediaBaseUrlField[TemplateFieldIDs.Type] = "Single-Line Text";
                }

                string currentSharedValue = mediaBaseUrlField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "0")
                {
                    mediaBaseUrlField[TemplateFieldIDs.Shared] = "0";
                }

                string currentUnversionedValue = mediaBaseUrlField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    mediaBaseUrlField[TemplateFieldIDs.Unversioned] = "1";
                }
            }

            Item publishHookUrlField = enterspeedSiteConfigSection.Children["Publish Hook Url"]
    ?? enterspeedSiteConfigSection.Add("Publish Hook Url", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedpublishHookUrlFieldID);

            using (new EditContext(publishHookUrlField))
            {
                publishHookUrlField.Appearance.Sortorder = 150;

                string currentTypeValue = publishHookUrlField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Single-Line Text", StringComparison.OrdinalIgnoreCase))
                {
                    publishHookUrlField[TemplateFieldIDs.Type] = "Single-Line Text";
                }

                string currentSharedValue = publishHookUrlField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "0")
                {
                    publishHookUrlField[TemplateFieldIDs.Shared] = "0";
                }

                string currentUnversionedValue = publishHookUrlField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    publishHookUrlField[TemplateFieldIDs.Unversioned] = "1";
                }
            }

            Item enablePreviewField = enterspeedSiteConfigSection.Children["Enable Preview"]
?? enterspeedSiteConfigSection.Add("Enable Preview", new TemplateID(TemplateIDs.TemplateField), EnterspeedIDs.Fields.EnterspeedEnablePreviewFieldID);

            using (new EditContext(enablePreviewField))
            {
                enablePreviewField.Appearance.Sortorder = 160;

                string currentTypeValue = enablePreviewField[TemplateFieldIDs.Type];
                if (!currentTypeValue.Equals("Checkbox", StringComparison.OrdinalIgnoreCase))
                {
                    enablePreviewField[TemplateFieldIDs.Type] = "Checkbox";
                }

                string currentSharedValue = enablePreviewField[TemplateFieldIDs.Shared];
                if (currentSharedValue != "0")
                {
                    enablePreviewField[TemplateFieldIDs.Shared] = "0";
                }

                string currentUnversionedValue = enablePreviewField[TemplateFieldIDs.Unversioned];
                if (currentUnversionedValue != "1")
                {
                    enablePreviewField[TemplateFieldIDs.Unversioned] = "1";
                }
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

            Item enterspeedSiteConfigTemplateItem = EnsureEnterspeedSiteConfigurationTemplate(EnsureEnterspeedTemplatesFolder(templatesSystemRoot));

            EnsureEnterspeedSiteConfigurationTemplateFields(enterspeedSiteConfigTemplateItem);
            Item enterspeedSiteConfigSection = EnsureEnterspeedSiteConfigurationTemplateDataSection(enterspeedSiteConfigTemplateItem);

            EnsureEnterspeedSiteConfigurationDataSectionFields(enterspeedSiteConfigSection);

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

            EnsureEnterspeedConfigurationItem(systemRoot, out var newEnterspeedConfigurationItem);

            if (newEnterspeedConfigurationItem != null)
            {
                _publishManager.PublishItem(newEnterspeedConfigurationItem, new[] { webDb }, new[] { language }, true, false, true);
            }
        }
    }
}