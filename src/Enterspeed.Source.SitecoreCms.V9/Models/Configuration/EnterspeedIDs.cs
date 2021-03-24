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

            public static class Fields
            {
                public static readonly ID EnterspeedApiBaseUrlFieldID = new ID(Guid.Parse("{8E3253A1-1700-4A44-B9F1-32F3785FC0E3}"));
                public static readonly ID EnterspeedApiKeyFieldID = new ID(Guid.Parse("{77EBAD36-9C7D-4C5D-808D-50A100D3FB26}"));
                public static readonly ID EnterspeedEnabledSitesFieldID = new ID(Guid.Parse("{F9ED1DE7-C846-4D77-949A-C6AA07B6E8E7}"));
            }
        }

        public static class Items
        {
            public static readonly ID EnterspeedConfigurationID = new ID(Guid.Parse("{432BD944-8029-4B2E-9856-DB2245F89EFA}"));
        }
    }
}