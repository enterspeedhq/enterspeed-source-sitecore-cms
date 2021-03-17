using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Source.Sdk.Api.Models.Properties;
using Sitecore.Collections;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Services
{
    public class EnterspeedPropertyService : IEnterspeedPropertyService
    {
        private const string MetaData = "metaData";

        private readonly IContentIdentityService _identityService;

        public EnterspeedPropertyService(
            IContentIdentityService identityService)
        {
            _identityService = identityService;
        }

        public IDictionary<string, IEnterspeedProperty> GetProperties(Item item)
        {
            IDictionary<string, IEnterspeedProperty> properties = ConvertFields(item.Fields);

            properties.Add(MetaData, CreateMetaData(item));

            return properties;
        }

        private IDictionary<string, IEnterspeedProperty> ConvertFields(FieldCollection fields)
        {
            var output = new Dictionary<string, IEnterspeedProperty>();

            if (fields == null || fields.Any() == false)
            {
                return output;
            }

            return output;
        }

        private IEnterspeedProperty CreateMetaData(Item item)
        {
            var paths = item.Paths.LongID.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            paths.RemoveAt(0); // Remove the first Sitecore item
            paths.RemoveAt(0); // Remove the first Content item

            int level = paths.IndexOf(item.ID.Guid.ToString("B").ToUpper());

            var metaData = new Dictionary<string, IEnterspeedProperty>
            {
                ["name"] = new StringEnterspeedProperty("name", item.Name),
                ["displayName"] = new StringEnterspeedProperty("name", item.DisplayName),
                ["culture"] = new StringEnterspeedProperty("culture", item.Language.Name),
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

            ids.RemoveAt(0); // Remove the first Sitecore item
            ids.RemoveAt(0); // Remove the first Content item

            var properties = ids
                .Select(x => new StringEnterspeedProperty(null, _identityService.GetId(x, item.Language.Name)))
                .ToArray();

            return properties;
        }
    }
}