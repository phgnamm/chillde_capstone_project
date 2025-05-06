using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.CancellationReasonModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CancellationReasonModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class CancellationReasonService : ICancellationReasonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;

        public CancellationReasonService(IUnitOfWork unitOfWork, IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
        }

        public async Task<ResponseModel> AddAsync(CancellationReasonAddModel model)
        {
            var existingReason = await _unitOfWork.CancellationReasonRepository.GetAllAsync(filter: _ => _.Name.ToLower().Trim().Equals(model.Name.ToLower().Trim()));

            if (existingReason.Data.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = $"Cancellation reason already exists."
                };
            }
            var cancellationReason = new CancellationReason
            {
                Name = model.Name,
                Value = model.Value,
                RoleType = model.RoleType,
                IsDeleted = false
            };

            await _unitOfWork.CancellationReasonRepository.AddAsync(cancellationReason);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel { Message = "Cancellation reason added successfully" }
                : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Failed to add cancellation reason" };
        }

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            var cancellationReason = await _unitOfWork.CancellationReasonRepository.GetAsync(id);
            if (cancellationReason == null) {
                return new ResponseModel { Code = StatusCodes.Status404NotFound, Message = "Not found" };
            }
            if (cancellationReason.IsDeleted == false)
            {
                _unitOfWork.CancellationReasonRepository.SoftRemove(cancellationReason);
            }
            else
            {
                cancellationReason.IsDeleted = false;
                _unitOfWork.CancellationReasonRepository.Update(cancellationReason);
            }
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0 ? new ResponseModel { Message = "Delete successfully" } : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Delete unsuccessfully"};
        }

        public async Task<ResponseModel> GetAllAsync(CancellationReasonFilterModel cancellationReasonFilterModel)
        {
            var cancellationLists = await _unitOfWork.CancellationReasonRepository.GetAllAsync(
         filter: x =>
             (!cancellationReasonFilterModel.IsDeleted.HasValue || x.IsDeleted == cancellationReasonFilterModel.IsDeleted) &&
             (string.IsNullOrEmpty(cancellationReasonFilterModel.Search) || x.Name.Contains(cancellationReasonFilterModel.Search) || x.Value.Equals(cancellationReasonFilterModel.Search)) &&
             (!cancellationReasonFilterModel.Role.HasValue || x.RoleType == cancellationReasonFilterModel.Role) &&
             (!cancellationReasonFilterModel.Value.HasValue || x.Value == cancellationReasonFilterModel.Value),
         order: x =>
         {
           switch (cancellationReasonFilterModel.Order.ToLower())
            {
                case "name":
                    return cancellationReasonFilterModel.OrderByDescending
                        ? x.OrderByDescending(x => x.Name)
                        : x.OrderBy(x => x.Name);

                case "value":
                    return cancellationReasonFilterModel.OrderByDescending
                        ? x.OrderByDescending(x => x.Value)
                        : x.OrderBy(x => x.Value);
                 case "applyForRole":
                     return cancellationReasonFilterModel.OrderByDescending
                         ? x.OrderByDescending(x => x.RoleType)
                         : x.OrderBy(x => x.RoleType);

                 case "creationDate":
                    return cancellationReasonFilterModel.OrderByDescending
                        ? x.OrderByDescending(x => x.CreationDate)
                        : x.OrderBy(x => x.CreationDate);

                case "isdeleted":
                    return cancellationReasonFilterModel.OrderByDescending
                        ? x.OrderByDescending(x => x.IsDeleted)
                        : x.OrderBy(x => x.IsDeleted);

                default:
                    return cancellationReasonFilterModel.OrderByDescending
                        ? x.OrderByDescending(x => x.Name)
                        : x.OrderBy(x => x.Name);
             }
         },
            pageIndex: cancellationReasonFilterModel.PageIndex,
            pageSize: cancellationReasonFilterModel.PageSize
     );

            var cancellationModels = cancellationLists.Data.Select(_ => new CancellationReasonModel
            {
                Id = _.Id,
                Name = _.Name,
                Value = _.Value,
                RoleType = _.RoleType,
                IsDeleted = _.IsDeleted,
                CreationDate = _.CreationDate
            });
            if (!cancellationModels.Any())
            {
                return new ResponseModel { Data = null };
            }

            var result = new Pagination<CancellationReasonModel>(cancellationModels.ToList(), cancellationReasonFilterModel.PageIndex,
                      cancellationReasonFilterModel.PageSize, cancellationLists.TotalCount);
            return new ResponseModel { Data = result };
        }

        public async Task<ResponseModel> UpdateAsync(Guid id, CancellationReasonAddModel model)
        {
            var cancellationReason = await _unitOfWork.CancellationReasonRepository.GetAsync(id);
            if (cancellationReason == null)
            {
                return new ResponseModel { Code = StatusCodes.Status404NotFound, Message = "Not found" };
            }
            _unitOfWork.CancellationReasonRepository.SoftRemove(cancellationReason);
            var cancellationReasonNew = new CancellationReason
            {
                Name = model.Name,
                Value = model.Value,
                RoleType = model.RoleType,
                IsDeleted = false
            };

            await _unitOfWork.CancellationReasonRepository.AddAsync(cancellationReasonNew);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel { Message = "Cancellation reason updated successfully" }
                : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Failed to update cancellation reason" };
        }
    }
}
