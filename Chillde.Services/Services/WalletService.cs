using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.WalletModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public WalletService(IUnitOfWork unitOfWork, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ResponseModel> Get()
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }
            var wallet = await _unitOfWork.WalletRepository.GetWalletByAccount(currentUserId.Value);
            var walletModel = new WalletModel
            {
                Balance = wallet?.Balance ?? 0.0m,
            };
            return new ResponseModel
            {
                Data = walletModel,
                Message = "Get wallet successfully"
            };
        }
    }
}
