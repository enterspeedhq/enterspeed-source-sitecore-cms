using System;
using System.Net.Http;
using Enterspeed.Source.Sdk.Api.Connection;
using Enterspeed.Source.Sdk.Api.Providers;
using Enterspeed.Source.Sdk.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Services
{
    public class SitecoreEnterspeedConnection : IEnterspeedConnection
    {
        private readonly EnterspeedConfiguration _configuration;
        private HttpClient _httpClientConnection;
        private DateTime? _connectionEstablishedDate;

        public SitecoreEnterspeedConnection(IEnterspeedConfigurationProvider configurationProvider)
        {
            _configuration = configurationProvider.Configuration;
        }

        public SitecoreEnterspeedConnection(EnterspeedConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ApiKey => _configuration.ApiKey;
        private string BaseUrl => _configuration.BaseUrl;
        private int ConnectionTimeout => _configuration.ConnectionTimeout;
        private string IngestVersion => _configuration.IngestVersion;

        public HttpClient HttpClientConnection
        {
            get
            {
                if (_httpClientConnection == null
                    || !_connectionEstablishedDate.HasValue
                    || ((DateTime.Now - _connectionEstablishedDate.Value).TotalSeconds > ConnectionTimeout))
                {
                    Connect();
                }

                return _httpClientConnection;
            }
        }

        public void Flush()
        {
            _httpClientConnection = null;
            _connectionEstablishedDate = null;
        }

        private void Connect()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("ApiKey is missing in the connection.");
            }

            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new Exception("BaseUrl is missing in the connection.");
            }

            _httpClientConnection = new HttpClient();
            _httpClientConnection.BaseAddress = new Uri(BaseUrl);
            _httpClientConnection.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
            _httpClientConnection.DefaultRequestHeaders.Add("Accept", "application/json");

            _connectionEstablishedDate = DateTime.Now;
        }
    }
}