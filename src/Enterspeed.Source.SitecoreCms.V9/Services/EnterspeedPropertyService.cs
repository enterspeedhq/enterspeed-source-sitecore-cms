﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Extensions;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.Formatters;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private const string MetaData = "metaData";
        private const string Renderings = "renderings";

        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IEnterspeedIdentityService _identityService;
        private readonly EnterspeedDateFormatter _dateFormatter;
        private readonly IEnumerable<IEnterspeedFieldValueConverter> _fieldValueConverters;
        private readonly IEnterspeedFieldConverter _fieldConverter;
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;

        public EnterspeedPropertyService(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IEnterspeedIdentityService identityService,
            EnterspeedDateFormatter dateFormatter,
            IEnumerable<IEnterspeedFieldValueConverter> fieldValueConverters,
            IEnterspeedFieldConverter fieldConverter,
            IEnterspeedIdentityService enterspeedIdentityService)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _identityService = identityService;
            _dateFormatter = dateFormatter;
            _fieldValueConverters = fieldValueConverters;
            _fieldConverter = fieldConverter;
            _enterspeedIdentityService = enterspeedIdentityService;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Item item)
        {
            if (item.IsDictionaryItem())
            {
                IDictionary<string, IEnterspeedProperty> dictionaryProperties = _fieldConverter.ConvertFields(item, null, _fieldValueConverters.ToList());

                return dictionaryProperties;
            }

            EnterspeedSiteInfo siteOfItem = _enterspeedConfigurationService
                .GetConfiguration()
                .GetSite(item);

            if (siteOfItem == null)
            {
                return null;
            }

            IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(item, siteOfItem, _fieldValueConverters.ToList());

            properties.Add(MetaData, CreateMetaData(item));
            properties.Add(Renderings, CreateRenderings(item));

            return properties;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(RenderingItem item)
        {
            IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(item.InnerItem, null, _fieldValueConverters.ToList());

            return properties;
        }

        private static List<Guid> GetContentPathIds(Item item)
        {
            List<Guid> ids = item
                .Paths
                .LongID
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .Where(x => x != ItemIDs.RootID.Guid && x != ItemIDs.ContentRoot.Guid)
                .ToList();

            return ids;
        }

        private IEnterspeedProperty CreateMetaData(Item item)
        {
            int level = GetContentPathIds(item).IndexOf(item.ID.Guid);

            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["name"] = new StringEnterspeedProperty("name", item.Name),
                ["displayName"] = new StringEnterspeedProperty("name", item.DisplayName),
                ["language"] = new StringEnterspeedProperty("language", item.Language.Name),
                ["sortOrder"] = new NumberEnterspeedProperty("sortOrder", item.Appearance.Sortorder),
                ["level"] = new NumberEnterspeedProperty("level", level),
                ["createDate"] = new StringEnterspeedProperty("createDate", _dateFormatter.FormatDate(item.Statistics.Created)),
                ["updateDate"] = new StringEnterspeedProperty("updateDate", _dateFormatter.FormatDate(item.Statistics.Updated)),
                ["fullPath"] = new ArrayEnterspeedProperty("fullPath", GetItemFullPath(item))
            };

            return new ObjectEnterspeedProperty(MetaData, metaData);
        }

        private IEnterspeedProperty CreateRenderings(Item item)
        {
            var renderingReferences = new List<ObjectEnterspeedProperty>();

            RenderingReference[] renderings = item.Visualization.GetRenderings(DeviceItem.ResolveDevice(item.Database), false);

            foreach (RenderingReference renderingReference in renderings)
            {
                if (renderingReference.RenderingItem == null)
                {
                    continue;
                }

                if (renderingReference.RenderingID.Guid == Guid.Empty)
                {
                    continue;
                }

                string placeholder;

                if (!string.IsNullOrEmpty(renderingReference.Placeholder))
                {
                    placeholder = renderingReference.Placeholder;
                }
                else
                {
                    placeholder = renderingReference.RenderingItem.Placeholder;
                }

                if (string.IsNullOrEmpty(placeholder))
                {
                    // We cannot continue not knowing the placeholder
                    continue;
                }

                var renderingProperties = new Dictionary<string, IEnterspeedProperty>
                {
                    ["name"] = new StringEnterspeedProperty("name", renderingReference.RenderingItem.Name),
                    ["placeholder"] = new StringEnterspeedProperty("placeholder", placeholder)
                };

                if (!string.IsNullOrEmpty(renderingReference.Settings.Parameters))
                {
                    NameValueCollection parameters = HttpUtility.ParseQueryString(renderingReference.Settings.Parameters);

                    var parameterItems = new List<StringEnterspeedProperty>();

                    foreach (string key in parameters)
                    {
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }

                        string value = parameters[key];
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        parameterItems.Add(new StringEnterspeedProperty(key, value));
                    }

                    if (parameters.Count > 0 && parameterItems.Count > 0)
                    {
                        renderingProperties.Add("parameters", new ArrayEnterspeedProperty("parameters", parameterItems.ToArray()));
                    }
                }

                if (!string.IsNullOrEmpty(renderingReference.Settings.DataSource))
                {
                    Item datasourceItem = item.Database.GetItem(renderingReference.Settings.DataSource, item.Language, Version.Latest);
                    if (datasourceItem != null && datasourceItem.Versions.Count > 0)
                    {
                        renderingProperties.Add("datasource", new StringEnterspeedProperty("datasource", _identityService.GetId(datasourceItem)));
                    }
                }

                renderingReferences.Add(new ObjectEnterspeedProperty(null, renderingProperties));
            }

            return new ArrayEnterspeedProperty(Renderings, renderingReferences.ToArray());
        }

        private IEnterspeedProperty[] GetItemFullPath(Item item)
        {
            List<Guid> ids = GetContentPathIds(item);

            var properties = ids
                .Select(x => new StringEnterspeedProperty(null, _identityService.GetId(x, item.Language)))
                .ToArray();

            return properties;
        }
    }
}