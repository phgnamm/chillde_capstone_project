using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.DepositModels;
using Chillde.Repositories.Models.ReputationLogModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.DepositModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Services.Services
{
    public class DepositService : IDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _claimService;

        public DepositService(IUnitOfWork unitOfWork,
            IMapper mapper, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimService = claimService;
        }

        public async Task<ResponseModel> GetAll(DepositFilterModel depositFilterModel)
        {
            try
            {
                var deposits = await _unitOfWork.DepositRepository.GetAllAsync(
                                filter: _ => _.CreatedById == depositFilterModel.AccountId,
                                pageIndex: depositFilterModel.PageIndex,
                                pageSize: depositFilterModel.PageSize
                );

                var depositModels = _mapper.Map<List<DepositModel>>(deposits.Data);

                var result = new Pagination<DepositModel>(depositModels, depositFilterModel.PageIndex,
                  depositFilterModel.PageSize, deposits.TotalCount);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Internal server error: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> WithDraw(decimal amount)
        {
            try
            {
                var currentUserId = _claimService.GetCurrentUserId!.Value;

                var account = await _unitOfWork.AccountRepository.GetAsync(currentUserId, x => x.Include(x => x.Wallet));

                if (account == null || account.Wallet == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Account or wallet not found."
                    };
                }

                if (account.Wallet.Balance < amount)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Insufficient balance."
                    };
                }

                account.Wallet.Balance -= amount;

                var deposit = new Deposit
                {
                    Amount = amount,
                    Type = DepositType.Withdraw,
                    Status = DepositStatus.Success,
                    WalletId = account.Wallet.Id,
                };
                
                await _unitOfWork.DepositRepository.AddAsync(deposit);

                _unitOfWork.WalletRepository.Update(account.Wallet);

                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Withdrawal successful.",
                    Data = new
                    {
                        NewBalance = account.Wallet.Balance
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Internal server error: {ex.Message}"
                };
            }
        }


    }
}
