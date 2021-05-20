using Enterspeed.Source.SitecoreCms.V9.Services;
using Sitecore.Data.Items;

namespace Enterspeed.Source.SitecoreCms.V9.Models.Mappers
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

        public SitecoreDictionaryEntity Map(Item input)
        {
            var output = new SitecoreDictionaryEntity
            {
                Id = _enterspeedIdentityService.GetId(input),
                Type = "dictionaryEntry",
                Properties = _enterspeedPropertyService.GetProperties(input)
            };

            return output;
        }
    }
}