using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Repositories.Models.WalletHistoryModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.WalletHistoryModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class WalletHistoryService : IWalletHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public WalletHistoryService(IUnitOfWork unitOfWork, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ResponseModel> GetAllWalletHistoryFromUser(WalletHistoryFilterModel walletHistoryFilterModel)
        {
            //var currentUserId = _claimService.GetCurrentUserId;
            //if (!currentUserId.HasValue)
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status401Unauthorized,
            //        Message = "Unauthorized."
            //    };
            //}

            var walletHistory = await _unitOfWork.TransactionRepository.GetAllAsync(
                filter: _ => 
                _.IsDeleted == walletHistoryFilterModel.IsDeleted && 
                (!walletHistoryFilterModel.AccountId.HasValue || _.CreatedById == walletHistoryFilterModel.AccountId),
                include: walletHistory => walletHistory.Include(_ => _.Wallet),
                pageIndex: walletHistoryFilterModel.PageIndex,
                pageSize: walletHistoryFilterModel.PageSize
            );

            if (walletHistory.Data == null || !walletHistory.Data.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "No wallet history found for this user."
                };
            }
            var walletHistoryModels = walletHistory.Data.Select(_ => new WalletHistoryModel
            {
                Id = _.Id,
                WalletId = _.WalletId,
                Amount = _.Amount ?? 0.0m,
                Status = _.Status,
               Type = _.Type
            }).ToList();

            var result = new Pagination<WalletHistoryModel>(walletHistoryModels, walletHistoryFilterModel.PageIndex,
                 walletHistoryFilterModel.PageSize, walletHistory.TotalCount);

            return new ResponseModel
            {
                Message = "Get wallet histories successfully",
                Data = result
            };
        }
    }
}
