using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Chillde.Services.Interfaces
{
    public interface IOfferService
    {
        Task<ResponseModel> GetAllAsync(OfferFilterModel filterParameter, Guid requestId, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> GetByIdAsync(Guid id, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> AddAsync(OfferAddModel model, Guid requestId, string sourceLanguageCode, string targetLanguageCode);
        Task<ResponseModel> UpdateAsync(Guid offerId, OfferUpdateModel model);
        Task<ResponseModel> DeleteAsync(Guid offerId);
    }
}
