using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.ReputationLogModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.DepositModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ReputationLogModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class ReputationLogService : IReputationLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReputationLogService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetAll(ReputationLogFilterModel reputationLogFilterModel)
        {
            try
            {
                var reputationLogs = await _unitOfWork.ReputationLogRepository.GetAllAsync(
                               filter: _ => _.AccountRoleId == reputationLogFilterModel.AccountRoleId,
                                pageIndex: reputationLogFilterModel.PageIndex,
                                pageSize: reputationLogFilterModel.PageSize,
                                include: _ => _.Include(_ => _.Order)
                );

                var reputationLogModels = reputationLogs.Data.Select(_ => new ReputationLogModel
                {
                    Id = _.Id,
                    OrderCode = _.Order.Code,
                    Reason = _.Reason,
                    PointChange = _.PointChange,
                    CreationDate = _.CreationDate
                }).ToList();

                var result = new Pagination<ReputationLogModel>(reputationLogModels, reputationLogFilterModel.PageIndex,
                  reputationLogFilterModel.PageSize, reputationLogs.TotalCount);

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
