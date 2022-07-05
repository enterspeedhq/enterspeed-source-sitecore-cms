using System;
using Sitecore.Data;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Configuration
{
    public static class EnterspeedIDs
    {
        public static class Templates
        {
            public static readonly ID EnterspeedFolderID = new ID(Guid.Parse("{3EBB1059-E90A-4DF4-82DB-E6092F0C3F45}"));
            public static readonly ID EnterspeedConfigurationID = new ID(Guid.Parse("{99BBFAAE-4C3C-440A-A80A-8756227D0AEE}"));
            public static readonly ID EnterspeedConfigurationDataSectionID = new ID(Guid.Parse("{0874F5FA-0441-4BC8-98B0-267052422AB8}"));
            public static readonly ID EnterspeedSiteConfigurationID = new ID(Guid.Parse("{f68dd470-ca5c-49b9-a946-4a88308c45d1}"));
            public static readonly ID EnterspeedSiteConfigurationDataSectionID = new ID(Guid.Parse("{60e7a64d-5177-40bc-9529-2f968fd5373b}"));
        }

        public static class Fields
        {
            public static readonly ID EnterspeedEnabledFieldID = new ID(Guid.Parse("{9D2D810C-B68A-43C3-BC0E-CCDFA144A06C}"));
            public static readonly ID EnterspeedApiBaseUrlFieldID = new ID(Guid.Parse("{8E3253A1-1700-4A44-B9F1-32F3785FC0E3}"));
            public static readonly ID EnterspeedApiKeyFieldID = new ID(Guid.Parse("{77EBAD36-9C7D-4C5D-808D-50A100D3FB26}"));
            public static readonly ID EnterspeedEnabledSitesFieldID = new ID(Guid.Parse("{F9ED1DE7-C846-4D77-949A-C6AA07B6E8E7}"));
            public static readonly ID EnterspeedEnabledDictionariesFieldID = new ID(Guid.Parse("{9CF467EB-0A4E-42FA-AB01-5FB7A006CD24}"));
            public static readonly ID EnterspeedSiteBaseUrlFieldID = new ID(Guid.Parse("{a5c257ad-5c76-40e4-9411-8a9cc98cbeb6}"));
            public static readonly ID EnterspeedMediaBaseUrlFieldID = new ID(Guid.Parse("{33ec5722-c98e-46dd-9c33-88d75cae8000}"));
            public static readonly ID EnterspeedpublishHookUrlFieldID = new ID(Guid.Parse("{37ec5722-c98e-46dd-9c33-88d75cbe8000}"));
            public static readonly ID EnterspeedEnablePreviewFieldID = new ID(Guid.Parse("{091f5494-317d-451e-9177-86527599dfd8}"));
            public static readonly ID EnterspeedPreviewBaseUrlFieldID = new ID(Guid.Parse("{92ee7860-ed9d-4803-b488-1494fd6395ab}"));
        }

        public static class Items
        {
            public static readonly ID EnterspeedConfigurationID = new ID(Guid.Parse("{432BD944-8029-4B2E-9856-DB2245F89EFA}"));
        }
    }
}