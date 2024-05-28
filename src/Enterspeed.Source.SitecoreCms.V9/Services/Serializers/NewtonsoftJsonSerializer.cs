﻿using Enterspeed.Source.Sdk.Api.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enterspeed.Source.SitecoreCms.V9.Services.Serializers
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