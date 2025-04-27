using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
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
            IQueryable<Transaction> transactionsQuery;

            if (transactionFilterModel.AccountId.HasValue)
            {
                var account = await _unitOfWork.AccountRepository.GetAsync(
                    transactionFilterModel.AccountId.Value,
                    include: _ => _.Include(a => a.Wallet)
                                  .ThenInclude(w => w.Transactions)
                                  .ThenInclude(t => t.Order)
                );

                if (account?.Wallet?.Transactions == null || !account.Wallet.Transactions.Any())
                {
                    return new ResponseModel
                    {
                        Data = null,
                        Message = "No transactions found for this account."
                    };
                }

                transactionsQuery = account.Wallet.Transactions.AsQueryable();
            }
            else
            {
                var walletHistory = await _unitOfWork.TransactionRepository.GetAllAsync(
                    filter: null,
                    include: t => t.Include(x => x.Wallet).Include(x => x.Order)
                );

                if (walletHistory.Data == null || !walletHistory.Data.Any())
                {
                    return new ResponseModel
                    {
                        Data = null,
                        Message = "No wallet history found."
                    };
                }

                transactionsQuery = walletHistory.Data.AsQueryable();
            }

            transactionsQuery = transactionsQuery.Where(t =>
                (!transactionFilterModel.IsDeleted.HasValue || t.IsDeleted == transactionFilterModel.IsDeleted) &&
                (!transactionFilterModel.TransactionStatus.HasValue || t.Status == (TransactionStatus)transactionFilterModel.TransactionStatus) &&
                (string.IsNullOrEmpty(transactionFilterModel.Search) ||
                    (t.Order != null && t.Order.Code.Contains(transactionFilterModel.Search)) ||
                    (t.Amount.HasValue && t.Amount.Value.ToString().Contains(transactionFilterModel.Search)))
            );

            transactionsQuery = transactionFilterModel.Order?.ToLower() switch
            {
                "ordercode" => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.Order.Code)
                    : transactionsQuery.OrderBy(x => x.Order.Code),
                "amount" => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.Amount)
                    : transactionsQuery.OrderBy(x => x.Amount),
                "transactiontype" => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.Type)
                    : transactionsQuery.OrderBy(x => x.Type),
                "creationdate" => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.CreationDate)
                    : transactionsQuery.OrderBy(x => x.CreationDate),
                "transactionstatus" => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.Status)
                    : transactionsQuery.OrderBy(x => x.Status),
                _ => transactionFilterModel.OrderByDescending
                    ? transactionsQuery.OrderByDescending(x => x.CreationDate)
                    : transactionsQuery.OrderBy(x => x.CreationDate)
            };

            var totalCount = transactionsQuery.Count();
            var pagedTransactions = transactionsQuery
                .Skip((transactionFilterModel.PageIndex - 1) * transactionFilterModel.PageSize)
                .Take(transactionFilterModel.PageSize)
                .ToList();

            var walletHistoryModels = pagedTransactions.Select(t => new WalletHistoryModel
            {
                Id = t.Id,
                WalletId = t.WalletId,
                OrderCode = t.Order?.Code ?? string.Empty,
                Amount = t.Amount ?? 0.0m,
                Description = t.Description ?? string.Empty,
                Status = t.Status,
                Type = t.Type,
                CreationDate = t.CreationDate
            }).ToList();

            var result = new Pagination<WalletHistoryModel>(walletHistoryModels,
                transactionFilterModel.PageIndex,
                transactionFilterModel.PageSize,
                totalCount);

            return new ResponseModel
            {
                Message = "Get wallet histories successfully",
                Data = result
            };
        }


    }
}
