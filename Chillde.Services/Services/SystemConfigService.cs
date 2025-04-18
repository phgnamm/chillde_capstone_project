using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Models.SystemConfigModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
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

        public async Task<ResponseModel> Update(Guid id, SystemConfigUpdateModel model)
        {
            try
            {
                var config = await _unitOfWork.SystemConfigRepository.GetAsync(id);
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
        x => !x.IsDeleted && (!model.Type.HasValue || x.EntityType == model.Type),
        q =>
        {
            switch (model.OrderOption)
            {
                case SortOptions.EntityType:
                    return model.OrderByDescending
                        ? q.OrderByDescending(x => x.EntityType)
                        : q.OrderBy(x => x.EntityType);

                case SortOptions.FieldName:
                    return model.OrderByDescending
                        ? q.OrderByDescending(x => x.FieldName)
                        : q.OrderBy(x => x.FieldName);

                case SortOptions.CreationDate:
                    return model.OrderByDescending
                        ? q.OrderByDescending(x => x.CreationDate)
                        : q.OrderBy(x => x.CreationDate);

                default:
                    return q.OrderBy(x => x.Id);
            }
        },
        include: null,
        1,
        1000
        );


            var mappedConfigs = configList.Data
                .Select(x => new SystemConfigModel
                {
                    Id = x.Id,
                    FieldName = ConfigKeyDisplayNames.DisplayNames
                        .FirstOrDefault(kvp => kvp.Key.ToString() == x.FieldName).Value ?? x.FieldName!,
                    EntityType = x.EntityType.ToString(),
                    Value = x.Value
                }).ToList();

            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                mappedConfigs = mappedConfigs
                    .Where(x => x.FieldName.Contains(model.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            var pagedConfigs = mappedConfigs
           .Skip((model.PageIndex - 1) * model.PageSize)
           .Take(model.PageSize)
           .ToList();
            if (!pagedConfigs.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Configuration does not exist",
                    Data = null
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get Configurations Successfully",
                Data = new Pagination<SystemConfigModel>
                (
                    pagedConfigs,
                    model.PageIndex,
                    model.PageSize,
                    mappedConfigs.Count
                )
            };
        }




        public async Task<ResponseModel> Get(SystemConfigKey key)
        {
            var config = await _unitOfWork.SystemConfigRepository.GetByKeyAsync(key);

            if (config == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Configuration does not exist",
                    Data = null
                };
            }
            string? value;
            if (config.Value is { } jsonDoc)
            {
                value = jsonDoc.RootElement.ToString();
            }
            else
            {
                value = config.Value?.ToString()?.Trim('"');
            }

            var result = new SystemConfigModel
            {
                FieldName = config.FieldName!,
                EntityType = config.EntityType.ToString(),
                Value = value!,
                CreationDate = config.CreationDate,
                IsDeleted = config.IsDeleted
            };

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Get Configuration Successfully",
                Data = result
            };
        }
    }
}
