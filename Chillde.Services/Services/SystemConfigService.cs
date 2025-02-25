using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SystemConfigModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SystemConfigService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Add(SystemConfigAddModel model)
        {
            try
            {
                var existingConfig = await _unitOfWork.SystemConfigRepository.GetByEntityTypeAsync(model.FieldName);
                if (existingConfig != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Configuration key already exists",
                        Data = false
                    };
                }

                var config = new SystemConfig
                {
                    FieldName = model.FieldName,
                    EntityType = model.EntityType,
                    Value = JsonSerializer.SerializeToDocument(model.Value)
                };

                await _unitOfWork.SystemConfigRepository.AddAsync(config);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Configuration added successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while adding configuration: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<ResponseModel> Update(SystemConfigAddModel model)
        {
            try
            {
                var config = await _unitOfWork.SystemConfigRepository.GetByEntityTypeAsync(model.FieldName);
                if (config == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Configuration not found",
                        Data = false
                    };
                }

                config.Value = JsonSerializer.SerializeToDocument(model.Value);
                _unitOfWork.SystemConfigRepository.Update(config);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Configuration updated successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while updating configuration: {ex.Message}",
                    Data = false
                };
            }
        }
        public async Task<ResponseModel> GetAll(SystemConfigFilterModel model)
        {
            var configList = await _unitOfWork.SystemConfigRepository.GetAllAsync(
                systemConfig => !systemConfig.IsDeleted,
                systemConfig =>
                    model.OrderByDescending
                        ? systemConfig.OrderByDescending(offer => offer.CreationDate)
                        : systemConfig.OrderBy(offer => offer.CreationDate),
                include: null,
                model.PageIndex,
                model.PageSize
            );

            if (configList == null || !configList.Data.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Configuration does not exist",
                    Data = null
                };
            }

            var result = configList.Data.Select(x => new SystemConfigModel
            {
                FieldName = x.FieldName!,
                EntityType = x.EntityType.ToString(),
                Value = x.Value
            }).ToList();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get All Configurations Successfully",
                Data = result
            };
        }
    }
}
