using Chillde.Repositories.Entities;
using Chillde.Repositories.Models.OfferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IOfferRepository : IGenericRepository<Offer>
    {
        Task<IEnumerable<OfferModel>> GetOffersWithTranslationsAsync(string targetLanguageCode, List<Guid> offerIds);
        Task<OfferModel?> GetOfferAsync(Guid id, string targetLanguageCode);
    }

}
