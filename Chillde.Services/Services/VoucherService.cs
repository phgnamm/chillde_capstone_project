using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.VoucherModels;
using Chillde.Repositories.Models.VoucherUsageModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Models.VoucherModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IClaimService _claimService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisHelper _redisHelper;
        private readonly IMapper _mapper;

        public VoucherService(IClaimService claimService, IUnitOfWork unitOfWork, IRedisHelper redisHelper, IMapper mapper)
        {
            _claimService = claimService;
            _unitOfWork = unitOfWork;
            _redisHelper = redisHelper;
            _mapper = mapper;
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

                 if (voucherAddModel.StartTime >= voucherAddModel.ExpiredTime)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "StartTime must be less than to ExpiredTime."
                    };
                }

                var newVoucher = new Voucher
                {
                    ReceiverId = voucherAddModel.ReceiverId,
                    Code = GenerateCodeHelper.GenerateVoucherCode(),
                    MinOrderRequired = voucherAddModel.MinOrderRequired ?? 0,
                    MinReputation = voucherAddModel.MinReputation ?? null,
                    DiscountValue = voucherAddModel.DiscountValue,
                    MinOrderValue = voucherAddModel.MinOrderValue ?? 0,
                    MaxDiscountValue = voucherAddModel.MaxDiscountValue,
                    TotalQuantity = voucherAddModel.TotalQuantity ?? null,
                    RemainingQuantity = voucherAddModel.TotalQuantity ?? null,
                    StartTime = voucherAddModel.StartTime,
                    ExpiredTime = voucherAddModel.ExpiredTime,
                    CreatedById = currentUserId.Value,
                    VoucherType = voucherAddModel.VoucherType,
                };
                if(voucherAddModel.ReceiverId != null)
                {
                    newVoucher.VoucherType = VoucherType.AdminToArtist;
                }
                await _unitOfWork.VoucherRepository.AddAsync(newVoucher);
                var result = await _unitOfWork.SaveChangeAsync();
                return result > 0
                    ? new ResponseModel { Message = "Create Voucher Successfully" }
                    : new ResponseModel { Message = "Create Voucher Unsuccessfully", Code = StatusCodes.Status400BadRequest };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResponseModel> StopVoucher(Guid id)
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

            if (existingVoucher.ExpiredTime <= DateTime.UtcNow)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Voucher already expired. Cannot stop."
                };
            }
            existingVoucher.VoucherStatus = Repositories.Enums.VoucherStatus.Expired;
            _unitOfWork.VoucherRepository.SoftRemove(existingVoucher);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel { Message = "Voucher has been stopped successfully." };
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

            var startTimeToCheck = voucherUpdateModel.StartTime ?? existingVoucher.StartTime;
            var expiredTimeToCheck = voucherUpdateModel.ExpiredTime ?? existingVoucher.ExpiredTime;

            if (voucherUpdateModel.ExpiredTime != null && voucherUpdateModel.ExpiredTime <= DateTime.UtcNow)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "ExpiredTime must be greater than now."
                };
            }

            if (voucherUpdateModel.StartTime != null || voucherUpdateModel.ExpiredTime != null)
            {
                if (startTimeToCheck > expiredTimeToCheck)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "StartTime must be less than or equal to ExpiredTime."
                    };
                }
            }

            if (hasUsed.Data.Any())
            {
                existingVoucher.VoucherStatus = Repositories.Enums.VoucherStatus.Expired;
                _unitOfWork.VoucherRepository.SoftRemove(existingVoucher);
                var newVoucher = new Voucher
                {
                    Code = GenerateCodeHelper.GenerateVoucherCode(),
                    ReceiverId = voucherUpdateModel.ReceiverId ??existingVoucher.ReceiverId,
                    MinOrderRequired = voucherUpdateModel.MinOrderRequired ?? existingVoucher.MinOrderRequired,
                    MinReputation = voucherUpdateModel.MinReputation ?? existingVoucher.MinReputation,
                    DiscountValue = voucherUpdateModel.DiscountValue > 0 ? voucherUpdateModel.DiscountValue : existingVoucher.DiscountValue,
                    MinOrderValue = voucherUpdateModel.MinOrderValue ?? existingVoucher.MinOrderValue,
                    MaxDiscountValue = voucherUpdateModel.MaxDiscountValue ?? existingVoucher.MaxDiscountValue,
                    TotalQuantity = voucherUpdateModel.TotalQuantity ?? existingVoucher.TotalQuantity,
                    RemainingQuantity = existingVoucher.RemainingQuantity,
                    StartTime = voucherUpdateModel.StartTime ?? existingVoucher.StartTime,
                    ExpiredTime = voucherUpdateModel.ExpiredTime ?? existingVoucher.ExpiredTime,
                    CreatedById = currentUserId.Value,
                    IsDeleted = voucherUpdateModel.IsDeleted ?? existingVoucher.IsDeleted,
                };

                await _unitOfWork.VoucherRepository.AddAsync(newVoucher);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel { Message = "Voucher has been used before. Old one soft-deleted. New voucher created." };
            }
            existingVoucher.ReceiverId = voucherUpdateModel.ReceiverId ?? existingVoucher.ReceiverId;
            existingVoucher.MinOrderRequired = voucherUpdateModel.MinOrderRequired ?? existingVoucher.MinOrderRequired;
            existingVoucher.MinReputation = voucherUpdateModel.MinReputation ?? existingVoucher.MinReputation;
            existingVoucher.DiscountValue = voucherUpdateModel.DiscountValue > 0
                ? voucherUpdateModel.DiscountValue
                : existingVoucher.DiscountValue;
            existingVoucher.MinOrderValue = voucherUpdateModel.MinOrderValue ?? existingVoucher.MinOrderValue;
            existingVoucher.MaxDiscountValue = voucherUpdateModel.MaxDiscountValue ?? existingVoucher.MaxDiscountValue;
            existingVoucher.TotalQuantity = voucherUpdateModel.TotalQuantity ?? existingVoucher.TotalQuantity;
            existingVoucher.RemainingQuantity = existingVoucher.RemainingQuantity;
            existingVoucher.StartTime = voucherUpdateModel.StartTime ?? existingVoucher.StartTime;
            existingVoucher.ExpiredTime = voucherUpdateModel.ExpiredTime ?? existingVoucher.ExpiredTime;
            existingVoucher.IsDeleted = voucherUpdateModel?.IsDeleted ?? existingVoucher.IsDeleted;

            _unitOfWork.VoucherRepository.Update(existingVoucher);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel { Message = "Voucher updated successfully." };
        }

        public async Task<ResponseModel> GetAll(VoucherFilterModel voucherFilterModel)
        {
            try
            {
                //var cacheKey = $"vouchers_{CacheTools.GenerateCacheKey(voucherFilterModel)}";

                //return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                Func<IQueryable<Voucher>, IQueryable<Voucher>> include = vouchers => vouchers
                             .Include(_ => _.Receiver)
                             .Include(_ => _.VoucherUsageLogs)
                             .ThenInclude(_ => _.Order);

                    var vouchers = await _unitOfWork.VoucherRepository.GetAllAsync(
                                    filter: voucher =>
                                         (!voucherFilterModel.ArtisanId.HasValue || voucher.VoucherStatus == voucherFilterModel.Status) &&
                                         (!voucherFilterModel.Status.HasValue || voucher.VoucherStatus == voucherFilterModel.Status) &&
                                         (voucher.IsDeleted == voucherFilterModel.IsDeleted) &&
                                         (!voucherFilterModel.MinDiscountValue.HasValue || voucher.DiscountValue >= voucherFilterModel.MinDiscountValue) &&
                                         (!voucherFilterModel.MaxDiscountValue.HasValue || voucher.DiscountValue <= voucherFilterModel.MaxDiscountValue) &&
                                         (!voucherFilterModel.VoucherType.HasValue || voucher.VoucherType == voucherFilterModel.VoucherType) &&
                                         (!voucherFilterModel.StartTime.HasValue || voucher.StartTime >= voucherFilterModel.StartTime) &&
                                         (!voucherFilterModel.ExpiredTime.HasValue || voucher.ExpiredTime <= voucherFilterModel.ExpiredTime) &&
                                         (!voucherFilterModel.MinReputation.HasValue ||
                                         (voucher.MinReputation.HasValue && voucher.MinReputation.Value >= voucherFilterModel.MinReputation)) &&
                                         (!voucherFilterModel.MinOrderRequired.HasValue ||
                                         (voucher.MinOrderRequired.HasValue && voucher.MinOrderRequired.Value >= voucherFilterModel.MinOrderRequired)),
                                     order: s =>
                                      {
                                          switch (voucherFilterModel.Order.ToLower())
                                          {
                                              case "userName":
                                                  return voucherFilterModel.OrderByDescending
                                                      ? s.OrderByDescending(s => s.Receiver!.Username)
                                                      : s.OrderBy(s => s.Receiver!.Username);
                                              case "discountValue":
                                                  return voucherFilterModel.OrderByDescending
                                                      ? s.OrderByDescending(s => s.DiscountValue)
                                                      : s.OrderBy(s => s.DiscountValue);
                                              case "expiredTime":
                                                  return voucherFilterModel.OrderByDescending
                                                      ? s.OrderByDescending(s => s.ExpiredTime)
                                                      : s.OrderBy(s => s.ExpiredTime);
                                              case "remainingQuantity":
                                                  return voucherFilterModel.OrderByDescending
                                                      ? s.OrderByDescending(s => s.RemainingQuantity)
                                                      : s.OrderBy(s => s.RemainingQuantity);
                                              case "status":
                                                  return voucherFilterModel.OrderByDescending
                                                      ? s.OrderByDescending(s => s.VoucherStatus)
                                                      : s.OrderBy(s => s.VoucherStatus);
                                              default:
                                                  return voucherFilterModel.OrderByDescending
                                                     ? s.OrderByDescending(s => s.CreationDate)
                                                     : s.OrderBy(s => s.CreationDate);
                                          }
                                      },
                                    include: include,
                                    pageIndex: voucherFilterModel.PageIndex,
                                    pageSize: voucherFilterModel.PageSize
                    );
                var voucherModels = vouchers.Data.Select(voucher => new VoucherModel
                {
                    Id = voucher.Id,
                    Code = voucher.Code,
                    ReceiverName = voucher.Receiver?.Username,
                    ReceiverId = voucher.ReceiverId,
                    MinReputation = voucher.MinReputation ?? null,
                    MinOrderRequired = voucher.MinOrderRequired ?? null,
                    DiscountValue = voucher.DiscountValue,
                    MinOrderValue = voucher.MinOrderValue ?? null,
                    MaxDiscountValue = voucher.MaxDiscountValue ?? null,
                    RemainingQuantity = voucher.RemainingQuantity ?? null,
                    TotalQuantity = voucher.TotalQuantity ?? null,
                    StartTime = voucher.StartTime,
                    ExpiredTime = voucher.ExpiredTime,
                    VoucherStatus = voucher.VoucherStatus,
                    VoucherUsageLogs = voucher.VoucherUsageLogs?.Select(log => new VoucherUsageLogModel
                    {
                        Id = log.Id,
                        VoucherId = log.VoucherId,
                        OrderId = log.OrderId,
                        DiscountValue = log.DiscountValue,
                        DiscountValueOrigin = log.DiscountValueOrigin,
                        UsageStatus = log.UsageStatus,
                        CreationDate = log.CreationDate,
                    }).ToList()
                }).ToList();


                var result = new Pagination<VoucherModel>(voucherModels, voucherFilterModel.PageIndex,
                      voucherFilterModel.PageSize, vouchers.TotalCount);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully.",
                        Data = result
                    };
                //});
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

        public async Task<ResponseModel> Get(Guid id)
        {
            var voucher = await _unitOfWork.VoucherRepository.GetAsync(id, include: voucher => voucher.Include(_ => _.VoucherUsageLogs));
            if (voucher == null)
            {
                return new ResponseModel { Message = "Voucher not found.", Code = StatusCodes.Status400BadRequest };
            }

            var voucherModel = _mapper.Map<VoucherModel>(voucher);

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Successfully.",
                Data = voucherModel
            };
        }

    }
}
