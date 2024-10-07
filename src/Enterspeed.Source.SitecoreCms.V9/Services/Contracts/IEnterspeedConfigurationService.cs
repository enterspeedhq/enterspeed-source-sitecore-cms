using System.Collections.Generic;
using Enterspeed.Source.SitecoreCms.V9.Models.Configuration;

namespace Enterspeed.Source.SitecoreCms.V9.Services.Contracts
{
    public interface IEnterspeedConfigurationService
    {
        List<EnterspeedSitecoreConfiguration> GetConfigurations();
    }
}