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
        Task<List<Offer>> GetOffersWithTranslationsAsync(string sourceLanguageCode, List<Guid> offerIds);
        Task<Offer> GetOfferAsync(Guid id, string sourceLanguageCode);
        Task<bool> RequestHasOffered(Guid id);
    }

}
