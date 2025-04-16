using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.DepositModels;
using Chillde.Repositories.Models.ReputationLogModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.DepositModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class DepositService : IDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepositService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetAll(DepositFilterModel depositFilterModel)
        {
            try
            {
                var deposits = await _unitOfWork.DepositRepository.GetAllAsync(
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
    }
}
