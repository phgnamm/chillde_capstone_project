using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.CancellationReasonModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CancellationReasonModels;
using Chillde.Services.Models.ResponseModels;
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
            _unitOfWork.CancellationReasonRepository.SoftRemove(cancellationReason);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0 ? new ResponseModel { Message = "Delete successfully" } : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Delete unsuccessfully"};
        }

        public async Task<ResponseModel> GetAllAsync()
        {
            var cancellationLists = await _unitOfWork.CancellationReasonRepository.GetAllAsync(filter: _ => _.IsDeleted ==  false);
            var cancellationModels = cancellationLists.Data.Select(_ => new CancellationReasonModel
            {
                Id = _.Id,
                Name = _.Name,
                Value = _.Value,
                RoleType = _.RoleType
            });
            if (!cancellationModels.Any())
            {
                return new ResponseModel { Data = null };

            }
            return new ResponseModel { Data = cancellationModels };
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
