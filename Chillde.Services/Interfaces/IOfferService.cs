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
        Task<ResponseModel> GetAll(OfferFilterModel offerFilterModel);
    }
}
