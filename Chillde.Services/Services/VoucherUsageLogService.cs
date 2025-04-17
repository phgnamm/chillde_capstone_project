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
                 (voucherUsage.UsageStatus == voucherUsageLogFilterModel.UsageStatus) &&
                 (voucherUsage.IsDeleted == voucherUsageLogFilterModel.IsDeleted) &&
                 (voucherUsage.DiscountValue >= voucherUsageLogFilterModel.MinDiscountValue) &&
                 (voucherUsage.DiscountValue <= voucherUsageLogFilterModel.MaxDiscountValue);

                var voucherUsages = await _unitOfWork.VoucherUsageLogRepository.GetAllAsync(
                                filter: filter,
                                pageIndex: voucherUsageLogFilterModel.PageIndex,
                                pageSize: voucherUsageLogFilterModel.PageSize
                );

                var voucherUsageModels = _mapper.Map<List<VoucherUsageLogModel>>(voucherUsages.Data);

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
