using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V9.Services.DataProperties;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private const string MetaData = "metaData";

        private readonly IEnterspeedConfigurationService _enterspeedConfigurationService;
        private readonly IContentIdentityService _identityService;
        private readonly IEnumerable<IEnterspeedFieldValueConverter> _fieldValueConverters;

        public EnterspeedPropertyService(
            IEnterspeedConfigurationService enterspeedConfigurationService,
            IContentIdentityService identityService,
            IEnumerable<IEnterspeedFieldValueConverter> fieldValueConverters)
        {
            _enterspeedConfigurationService = enterspeedConfigurationService;
            _identityService = identityService;
            _fieldValueConverters = fieldValueConverters;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Item item)
        {
            EnterspeedSiteInfo siteOfItem = _enterspeedConfigurationService
                .GetConfiguration()
                .SiteInfo
                .FirstOrDefault(x => x.IsItemOfSite(item));

            IDictionary<string, IEnterspeedProperty> properties = ConvertFields(item, siteOfItem);

            properties.Add(MetaData, CreateMetaData(item));

            return properties;
        }

        private IDictionary<string, IEnterspeedProperty> ConvertFields(Item item, EnterspeedSiteInfo siteInfo)
        {
            var output = new Dictionary<string, IEnterspeedProperty>();

            FieldCollection fieldsCollection = item.Fields;
            if (fieldsCollection == null || fieldsCollection.Any() == false)
            {
                return output;
            }

            // Exclude system fields
            List<Field> fields = fieldsCollection.Where(field =>
                    field.InnerItem.Paths.FullPath.StartsWith("/sitecore/templates/system", StringComparison.OrdinalIgnoreCase) == false)
                .ToList();

            if (fields.Any() == false)
            {
                return output;
            }

            foreach (Field field in fields)
            {
                if (field == null)
                {
                    continue;
                }

                IEnterspeedFieldValueConverter converter = _fieldValueConverters.FirstOrDefault(x => x.CanConvert(field));

                var value = converter?.Convert(item, field, siteInfo);
                if (value == null)
                {
                    continue;
                }

                output.Add(field.Name, value);
            }

            return output;
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
                ["createDate"] = new StringEnterspeedProperty("createDate", item.Statistics.Created.ToString("yyyy-MM-ddTHH:mm:ss")),
                ["updateDate"] = new StringEnterspeedProperty("updateDate", item.Statistics.Updated.ToString("yyyy-MM-ddTHH:mm:ss")),
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