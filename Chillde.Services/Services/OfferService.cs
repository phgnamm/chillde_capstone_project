using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.WalletHistoryModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.WalletHistoryModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public OfferService(IUnitOfWork unitOfWork, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ResponseModel> GetAll(OfferFilterModel offerFilterModel)
        {
            var offers = await _unitOfWork.OfferRepository.GetAllAsync(
               filter: _ => _.IsDeleted == offerFilterModel.IsDeleted,
               include: offer => offer.Include(_ => _.Service) .Include(_ => _.CreatedBy),
               pageIndex: offerFilterModel.PageIndex,
               pageSize: offerFilterModel.PageSize
           );

            if (offer.Data == null || !offer.Data.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "No offer found."
                };
            }
            var offerModels = offers.Data.Select(_ => new OfferModel
            {
                Id = _.Id,
                RequestId = _.RequestId,
                ServiceId = _.ServiceId,
                CreatedById = _.CreatedById,
                ArtistName = _.CreatedBy.FirstName + " " + _.CreatedBy.LastName,
                ServiceName = _.Service.Name ?? "",
                Message = _.Message ?? "",
                Status = _.Status,
            }).ToList();
            var result = new Pagination<OfferModel>(offerModels, offerFilterModel.PageIndex,
                offerFilterModel.PageSize, offers.TotalCount);

            return new ResponseModel
            {
                Message = "Get offers successfully",
                Data = result
            };
        }
    }
}
