using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Services.Contracts
{
    public interface IEnterspeedConfigurationService
    {
        List<EnterspeedSitecoreConfiguration> GetConfigurations();
    }
}