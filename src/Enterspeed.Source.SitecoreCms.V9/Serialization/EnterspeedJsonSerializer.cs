using Enterspeed.Source.Sdk.Api.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enterspeed.Source.SitecoreCms.V9.Serialization
{
    public class EnterspeedJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public EnterspeedJsonSerializer()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(_serializerSettings, _serializerSettings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
        }
    }
}