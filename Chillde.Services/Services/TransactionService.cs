using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Repositories.Models.WalletHistoryModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Chillde.Services.Models.WalletHistoryModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public TransactionService(IUnitOfWork unitOfWork, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ResponseModel> GetAllTransactionsFromUser(TransactionFilterModel transactionFilterModel)
        {

            //var currentUserId = _claimService.GetCurrentUserId;
            var walletHistory = await _unitOfWork.TransactionRepository.GetAllAsync(
                  filter: _ =>
                  (!transactionFilterModel.IsDeleted.HasValue || _.IsDeleted == transactionFilterModel.IsDeleted) &&
                  (!transactionFilterModel.AccountId.HasValue || _.CreatedById == transactionFilterModel.AccountId) &&
                  //(!transactionFilterModel.TransactionStatus.HasValue || _.Status == transactionFilterModel.TransactionStatus) &&
                                          (string.IsNullOrEmpty(transactionFilterModel.Search) || (
                                              _.Order.Code.Contains(transactionFilterModel.Search) ||
                                              _.Amount.Equals(transactionFilterModel.Search)
                                          )),
                    order: s =>
                    {
                        switch ((transactionFilterModel.Order?.ToLower()))
                        {
                            case "orderCode":
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(x => x.Order.Code)
                                    : s.OrderBy(x => x.Order.Code);

                            case "amount":
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(x => x.Amount)
                                    : s.OrderBy(x => x.Amount);

                            case "transactionType":
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(x => x.Type)
                                    : s.OrderBy(x => x.Type);

                            case "creationDate":
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(x => x.CreationDate)
                                    : s.OrderBy(x => x.CreationDate);

                            case "transactionStatus":
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(s => s.Status)
                                    : s.OrderBy(s => s.Status);                      

                            default:
                                return transactionFilterModel.OrderByDescending
                                    ? s.OrderByDescending(x => x.CreationDate)
                                    : s.OrderBy(x => x.CreationDate);
                        }
                    },
                  include: walletHistory => walletHistory.Include(_ => _.Wallet) .Include(_ => _.Order),
                  pageIndex: transactionFilterModel.PageIndex,
                  pageSize: transactionFilterModel.PageSize
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
                OrderCode = _.Order.Code,
                Amount = _.Amount ?? 0.0m,
                Status = _.Status,
               Type = _.Type
            }).ToList();

            var result = new Pagination<WalletHistoryModel>(walletHistoryModels, transactionFilterModel.PageIndex,
                 transactionFilterModel.PageSize, walletHistory.TotalCount);

            return new ResponseModel
            {
                Message = "Get wallet histories successfully",
                Data = result
            };
        }

    }
}
