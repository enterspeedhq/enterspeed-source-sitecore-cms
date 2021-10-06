using Enterspeed.Source.Sdk.Api.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Serializers
{
        public class NewtonsoftJsonSerializer : IJsonSerializer
        {
            private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            public string Serialize(object value)
            {
                return JsonConvert.SerializeObject(value, _settings);
            }

            public T Deserialize<T>(string value)
            {
                return JsonConvert.DeserializeObject<T>(value, _settings);
            }
    }
}