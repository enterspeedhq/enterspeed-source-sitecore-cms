using Enterspeed.Source.Sdk.Api.Models;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
{
    public interface IEntityModelMapper<in TInput, out TEntity>
        where TEntity : IEnterspeedEntity
    {
        TEntity Map(TInput renderingItem);
    }
}