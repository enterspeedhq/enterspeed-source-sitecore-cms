using Enterspeed.Source.SitecoreCms.V8.Models.Configuration;
using Enterspeed.Source.SitecoreCms.V8.Services.Contracts;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V8.Models.Mappers
{
    public class SitecoreDictionaryEntityModelMapper : IEntityModelMapper<Item, SitecoreDictionaryEntity>
    {
        private readonly IEnterspeedIdentityService _enterspeedIdentityService;
        private readonly IEnterspeedPropertyService _enterspeedPropertyService;

        public SitecoreDictionaryEntityModelMapper(
            IEnterspeedIdentityService enterspeedIdentityService,
            IEnterspeedPropertyService enterspeedPropertyService)
        {
            _enterspeedIdentityService = enterspeedIdentityService;
            _enterspeedPropertyService = enterspeedPropertyService;
        }

        public SitecoreDictionaryEntity Map(Item input, EnterspeedSitecoreConfiguration configuration)
        {
            var output = new SitecoreDictionaryEntity
            {
                Id = _enterspeedIdentityService.GetId(input),
                Type = "dictionaryEntry",
                Properties = _enterspeedPropertyService.GetProperties(input, configuration)
            };

            return output;
        }

        public SitecoreDictionaryEntity MapWithUrl(Item input, EnterspeedSitecoreConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}