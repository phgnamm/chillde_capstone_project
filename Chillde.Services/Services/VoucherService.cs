using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IClaimService _claimService;
        private readonly IUnitOfWork _unitOfWork;

        public VoucherService(IClaimService claimService, IUnitOfWork unitOfWork)
        {
            _claimService = claimService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Add(VoucherAddModel voucherAddModel)
        {
            try
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
                if (voucherAddModel.ExpiredTime <= DateTime.UtcNow)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "ExpiredTime must be greater than now."
                    };
                }
                var newVoucher = new Voucher
                {
                    Id = Guid.NewGuid(),
                    ReceiverId = voucherAddModel.ReceiverId ?? null,
                    Code = GenerateCodeHelper.GenerateVoucherCode(),
                    MinOrderRequired = voucherAddModel.MinOrderRequired ?? 0,
                    MinReputation = voucherAddModel.MinReputation ?? null,
                    DiscountValue = voucherAddModel.DiscountValue,
                    MinOrderValue = voucherAddModel.MinOrderValue ?? 0,
                    MaxDiscountValue = voucherAddModel.MaxDiscountValue ?? null,
                    TotalQuantity = voucherAddModel.TotalQuantity ?? null,
                    RemainingQuantity = voucherAddModel.TotalQuantity ?? voucherAddModel.TotalQuantity ?? null,
                    ExpiredTime = voucherAddModel.ExpiredTime,
                    CreatedById = currentUserId.Value,
                };

                await _unitOfWork.VoucherRepository.AddAsync(newVoucher);
                var result = await _unitOfWork.SaveChangeAsync();
                return result > 0 ? new ResponseModel { Message = "Create Voucher Successfully" } : new ResponseModel { Message = "Create Voucher Unsuccessfully", Code = StatusCodes.Status400BadRequest };
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResponseModel> Update(Guid id, VoucherUpdateModel voucherUpdateModel)
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
            var existingVoucher = await _unitOfWork.VoucherRepository.GetAsync(id);
            if (existingVoucher == null)
            {
                return new ResponseModel { Message = "Voucher not found.", Code = StatusCodes.Status400BadRequest };
            }
            var hasUsed = await _unitOfWork.VoucherUsageLogRepository.GetAllAsync(filter: _ => _.VoucherId == id);
            if (hasUsed.Data.Any()) {
                return new ResponseModel { Message = "Voucher cannot be edited because it has been used.", Code = StatusCodes.Status400BadRequest };
            }
            if (voucherUpdateModel.ExpiredTime <= DateTime.UtcNow)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "ExpiredTime must be greater than now."
                };
            }
            existingVoucher.MinOrderRequired = voucherUpdateModel.MinOrderRequired ?? existingVoucher.MinOrderRequired;
            existingVoucher.MinReputation = voucherUpdateModel.MinReputation ?? existingVoucher.MinReputation;
            existingVoucher.DiscountValue = voucherUpdateModel.DiscountValue > 0
                ? voucherUpdateModel.DiscountValue
                : existingVoucher.DiscountValue;
            existingVoucher.MinOrderValue = voucherUpdateModel.MinOrderValue ?? existingVoucher.MinOrderValue;
            existingVoucher.MaxDiscountValue = voucherUpdateModel.MaxDiscountValue ?? existingVoucher.MaxDiscountValue;
            existingVoucher.TotalQuantity = voucherUpdateModel.TotalQuantity ?? existingVoucher.TotalQuantity;
            existingVoucher.RemainingQuantity = voucherUpdateModel.RemainingQuantity ?? existingVoucher.RemainingQuantity;
            existingVoucher.ExpiredTime = voucherUpdateModel.ExpiredTime;

            existingVoucher.ModificationDate = DateTime.UtcNow;

            _unitOfWork.VoucherRepository.Update(existingVoucher);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel { Message = "Voucher updated successfully." };
        }
    }
}
