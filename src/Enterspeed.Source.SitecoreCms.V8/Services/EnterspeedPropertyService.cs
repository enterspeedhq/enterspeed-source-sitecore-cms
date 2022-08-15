using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V8.Extensions;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Enterspeed.Source.SitecoreCms.V8.Services.DataProperties;
using Enterspeed.Source.SitecoreCms.V8.Services.DataProperties.Formatters;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Security.Accounts;
using Version = Sitecore.Data.Version;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private const string MetaData = "metaData";
        private const string Renderings = "renderings";

        private readonly IEnterspeedIdentityService _identityService;
        private readonly EnterspeedDateFormatter _dateFormatter;
        private readonly IEnumerable<IEnterspeedFieldValueConverter> _fieldValueConverters;
        private readonly IEnterspeedFieldConverter _fieldConverter;
        private readonly BaseItemManager _itemManager;
        private readonly BaseUserManager _userManager;

        public EnterspeedPropertyService(
            IEnterspeedIdentityService identityService,
            EnterspeedDateFormatter dateFormatter,
            IEnumerable<IEnterspeedFieldValueConverter> fieldValueConverters,
            IEnterspeedFieldConverter fieldConverter,
            BaseItemManager itemManager,
            BaseUserManager userManager)
        {
            _identityService = identityService;
            _dateFormatter = dateFormatter;
            _fieldValueConverters = fieldValueConverters;
            _fieldConverter = fieldConverter;
            _itemManager = itemManager;
            _userManager = userManager;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Item item,  EnterspeedSitecoreConfiguration configuration)
        {
            if (item.IsDictionaryItem())
            {
                IDictionary<string, IEnterspeedProperty> dictionaryProperties = _fieldConverter.ConvertFields(item, null, _fieldValueConverters.ToList(), configuration);
                dictionaryProperties.Add(MetaData, CreateDictionaryMetaData(item));

                return dictionaryProperties;
            }

            EnterspeedSiteInfo siteOfItem = configuration.GetSite(item);

            if (siteOfItem == null)
            {
                return null;
            }

            IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(item, siteOfItem, _fieldValueConverters.ToList(), configuration);

            properties.Add(MetaData, CreateMetaData(item));

            IEnterspeedProperty renderings = CreateRenderings(item);
            if (renderings != null)
            {
                properties.Add(Renderings, renderings);
            }

            return properties;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(RenderingItem item, EnterspeedSitecoreConfiguration configuration)
        {
            IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(item.InnerItem, null, _fieldValueConverters.ToList(), configuration);

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

        private IEnterspeedProperty CreateDictionaryMetaData(Item item)
        {
            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["language"] = new StringEnterspeedProperty("language", item.Language.Name),
                ["createDate"] = new StringEnterspeedProperty("createDate", _dateFormatter.FormatDate(item.Statistics.Created)),
                ["updateDate"] = new StringEnterspeedProperty("updateDate", _dateFormatter.FormatDate(item.Statistics.Updated)),
                ["updatedBy"] = new StringEnterspeedProperty("updatedBy", item.Statistics.UpdatedBy),
                ["fullPath"] = new ArrayEnterspeedProperty("fullPath", GetItemFullPath(item)),
                ["languages"] = new ArrayEnterspeedProperty("languages", GetAvailableLanguagesOfItem(item)),
            };

            return new ObjectEnterspeedProperty(MetaData, metaData);
        }

        private IEnterspeedProperty CreateMetaData(Item item)
        {
            int level = GetContentPathIds(item).IndexOf(item.ID.Guid);

            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["name"] = new StringEnterspeedProperty("name", item.Name),
                ["displayName"] = new StringEnterspeedProperty("displayName", item.DisplayName),
                ["sitecoreId"] = new StringEnterspeedProperty("name", item.ID.ToString()),
                ["language"] = new StringEnterspeedProperty("language", item.Language.Name),
                ["sortOrder"] = new NumberEnterspeedProperty("sortOrder", item.Appearance.Sortorder),
                ["level"] = new NumberEnterspeedProperty("level", level),
                ["createDate"] = new StringEnterspeedProperty("createDate", _dateFormatter.FormatDate(item.Statistics.Created)),
                ["updateDate"] = new StringEnterspeedProperty("updateDate", _dateFormatter.FormatDate(item.Statistics.Updated)),
                ["updatedBy"] = new StringEnterspeedProperty("updatedBy", item.Statistics.UpdatedBy),
                ["fullPath"] = new ArrayEnterspeedProperty("fullPath", GetItemFullPath(item)),
                ["languages"] = new ArrayEnterspeedProperty("languages", GetAvailableLanguagesOfItem(item)),
                ["isAccessRestricted"] = GetIsAccessRestricted(item),
                ["accessRestrictions"] = GetAccessRestrictions(item)
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
                    ["name"] = new StringEnterspeedProperty("name", renderingReference.RenderingItem.Name.Replace(" ", string.Empty)),
                    ["placeholder"] = new StringEnterspeedProperty("placeholder", placeholder.Replace(" ", string.Empty))
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

            if (!renderingReferences.Any())
            {
                return null;
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

        private IEnterspeedProperty[] GetAvailableLanguagesOfItem(Item item)
        {
            var languages = new List<StringEnterspeedProperty>();

            foreach (Language language in item.Languages)
            {
                Item itemInLanguage = _itemManager.GetItem(item.ID, language, Version.Latest, item.Database);
                if (itemInLanguage == null ||
                    itemInLanguage.Versions.Count == 0)
                {
                    continue;
                }

                languages.Add(new StringEnterspeedProperty(null, language.Name));
            }

            return languages.ToArray();
        }

        private IEnterspeedProperty GetIsAccessRestricted(Item item)
        {
            bool isAccessRestricted;

            var allUsers = _userManager.GetUsers().ToList();
            var anonymous = allUsers.Single(x => x.Name.Equals("extranet\\anonymous", StringComparison.OrdinalIgnoreCase));
            using (new UserSwitcher(anonymous))
            {
                isAccessRestricted = !item.Access.CanRead();
            }

            return new BooleanEnterspeedProperty("isAccessRestricted", isAccessRestricted);
        }

        private IEnterspeedProperty GetAccessRestrictions(Item item)
        {
            var readAccess = new Dictionary<string, bool>();

            foreach (var user in _userManager.GetUsers())
            {
                using (new UserSwitcher(user))
                {
                    var canRead = item.Access.CanRead();

                    var userName = user.LocalName.ToLower();
                    if (readAccess.ContainsKey(userName))
                    {
                        readAccess[userName] = canRead;
                    }
                    else
                    {
                        readAccess.Add(userName, canRead);
                    }
                }
            }

            var accessRestrictionItems = new List<BooleanEnterspeedProperty>();

            if (readAccess.Any(x => !x.Value))
            {
                var usersWithRestrictedAccess = readAccess.Where(x => !x.Value).ToList();

                accessRestrictionItems.AddRange(usersWithRestrictedAccess.Select(x => new BooleanEnterspeedProperty(x.Key, x.Value)));
            }

            return new ArrayEnterspeedProperty("accessRestrictions", accessRestrictionItems.ToArray());
        }
    }
}