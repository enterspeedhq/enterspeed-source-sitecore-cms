using System.Collections.Generic;
using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.Sdk.Api.Models.Properties;

namespace Enterspeed.Source.SitecoreCms.V9.Models
{
    public class SitecoreDictionaryEntity : IEnterspeedEntity<IDictionary<string, IEnterspeedProperty>>
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Url => null;
        public string[] Redirects => new string[0];
        public string ParentId => null;
        public IDictionary<string, IEnterspeedProperty> Properties { get; set; }
    }
}