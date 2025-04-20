using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.VoucherModels;
using Chillde.Repositories.Models.VoucherUsageModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Chillde.Services.Models.VoucherUsageLogModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class VoucherUsageLogService : IVoucherUsageLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranslationService _translationService;
        private readonly IMapper _mapper;
        private readonly IBadWordFilterService _badWordFilterService;
        private readonly IPackageService _packageService;
        private readonly IRedisHelper _redisHelper;

        public VoucherUsageLogService(IUnitOfWork unitOfWork,
            ITranslationService translationService,
            IMapper mapper,
            IBadWordFilterService badWordFilterService,
            IPackageService packageService,
            IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
            _packageService = packageService;
            _redisHelper = redisHelper;
        }

        public async Task<ResponseModel> GetAll(VoucherUsageLogFilterModel voucherUsageLogFilterModel)
        {
            try
            {
                //var cacheKey = $"vouchers_{CacheTools.GenerateCacheKey(voucherFilterModel)}";

                //return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                Expression<Func<VoucherUsageLog, bool>> filter = voucherUsage =>
                 (!voucherUsageLogFilterModel.UsageStatus.HasValue || voucherUsage.UsageStatus == voucherUsageLogFilterModel.UsageStatus) &&
                 (voucherUsage.IsDeleted == voucherUsageLogFilterModel.IsDeleted) &&
                 (!voucherUsageLogFilterModel.MinDiscountValue.HasValue || voucherUsage.DiscountValue >= voucherUsageLogFilterModel.MinDiscountValue) &&
                 (!voucherUsageLogFilterModel.AccountId.HasValue || voucherUsage.CreatedById == voucherUsageLogFilterModel.AccountId) &&
                 (!voucherUsageLogFilterModel.MaxDiscountValue.HasValue || voucherUsage.DiscountValue <= voucherUsageLogFilterModel.MaxDiscountValue) &&
                 (!voucherUsageLogFilterModel.OrderId.HasValue || voucherUsage.OrderId == voucherUsageLogFilterModel.OrderId);

                var voucherUsages = await _unitOfWork.VoucherUsageLogRepository.GetAllAsync(
                                filter: filter,
                                include: _ => _.Include(_ => _.Order).Include(_ => _.Voucher),
                                  order: s =>
                                  {
                                      switch (voucherUsageLogFilterModel.Order.ToLower())
                                      {
                                          case "voucherCode":
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                  ? s.OrderByDescending(s => s.Voucher!.Code)
                                                  : s.OrderBy(s => s.Voucher!.Code);
                                          case "discountValue":
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                  ? s.OrderByDescending(s => s.DiscountValue)
                                                  : s.OrderBy(s => s.DiscountValue);
                                          case "discountValueOrigin":
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                  ? s.OrderByDescending(s => s.DiscountValueOrigin)
                                                  : s.OrderBy(s => s.DiscountValueOrigin);
                                          case "usageStatus":
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                  ? s.OrderByDescending(s => s.UsageStatus)
                                                  : s.OrderBy(s => s.UsageStatus);
                                          case "orderCode":
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                  ? s.OrderByDescending(s => s.Order!.Code)
                                                  : s.OrderBy(s => s.Order!.Code);
                                          default:
                                              return voucherUsageLogFilterModel.OrderByDescending
                                                 ? s.OrderByDescending(s => s.CreationDate)
                                                 : s.OrderBy(s => s.CreationDate);
                                      }
                                  },
                                pageIndex: voucherUsageLogFilterModel.PageIndex,
                                pageSize: voucherUsageLogFilterModel.PageSize
                );

                var voucherUsageModels = voucherUsages.Data.Select(v => new VoucherUsageLogModel
                {
                    Id = v.Id,
                    VoucherCode = v.Voucher.Code,
                    OrderCode = v.Order.Code,
                    CreatedById = v.CreatedById,
                    CreationDate = v.CreationDate,
                    ModifiedById = v.ModifiedById,
                    ModificationDate = v.ModificationDate,
                    IsDeleted = v.IsDeleted,
                    DiscountValue = v.DiscountValue,
                    DiscountValueOrigin = v.DiscountValueOrigin,
                    UsageStatus = v.UsageStatus
                }).ToList();

                var result = new Pagination<VoucherUsageLogModel>(voucherUsageModels, voucherUsageLogFilterModel.PageIndex,
                  voucherUsageLogFilterModel.PageSize, voucherUsages.TotalCount);

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
    }
}
