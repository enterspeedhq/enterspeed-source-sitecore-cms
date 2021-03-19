using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties.Formatters;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private const string MetaData = "metaData";

        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IContentIdentityService _identityService;
        private readonly EnterspeedDateFormatter _dateFormatter;
        private readonly IEnterspeedFieldConverter _fieldConverter;

        public EnterspeedPropertyService(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IContentIdentityService identityService,
            EnterspeedDateFormatter dateFormatter,
            IEnterspeedFieldConverter fieldConverter)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _identityService = identityService;
            _dateFormatter = dateFormatter;
            _fieldConverter = fieldConverter;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Item item)
        {
            EnterspeedSiteInfo siteOfItem = _enterspeedConfigurationService
                .GetConfiguration()
                .GetSite(item);

            IDictionary<string, IEnterspeedProperty> properties = _fieldConverter.ConvertFields(item, siteOfItem);

            properties.Add(MetaData, CreateMetaData(item));

            return properties;
        }

        private IEnterspeedProperty CreateMetaData(Item item)
        {
            var paths = item.Paths.LongID.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // TODO - KEK: revisit / refactor to more pretty solution
            paths.RemoveAt(0); // Remove the first Sitecore item
            paths.RemoveAt(0); // Remove the first Content item

            int level = paths.IndexOf(item.ID.Guid.ToString("B").ToUpper());

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

            return new ObjectEnterspeedProperty("metaData", metaData);
        }

        private IEnterspeedProperty[] GetItemFullPath(Item item)
        {
            List<Guid> ids = item
                .Paths
                .LongID
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .ToList();

            // TODO - KEK: revisit / refactor to more pretty solution
            ids.RemoveAt(0); // Remove the first Sitecore item
            ids.RemoveAt(0); // Remove the first Content item

            var properties = ids
                .Select(x => new StringEnterspeedProperty(null, _identityService.GetId(x, item.Language)))
                .ToArray();

            return properties;
        }
    }
}