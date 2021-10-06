using Enterspeed.Source.Sdk.Api.Models;
using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;

namespace Enterspeed.Source.SitecoreCms.V8.Models.Mappers
{
    public interface IEntityModelMapper<in TInput, out TEntity>
        where TEntity : IEnterspeedEntity
    {
        TEntity Map(TInput input, EnterspeedSitecoreConfiguration configuration);
    }
}