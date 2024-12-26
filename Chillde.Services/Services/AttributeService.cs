using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AttributeModels;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.AttributeModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class AttributeService : IAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttributeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Add(AttributeAddModel attributeAddModel)
        {
            var attribute = new Repositories.Entities.Attribute
            {
                Id = Guid.NewGuid(),
                Name = attributeAddModel.Name,
                Type = attributeAddModel.Type,
                IsRequired = false, 
                CreationDate = DateTime.UtcNow
            };

            if (attributeAddModel.AttributeValueModels != null && attributeAddModel.AttributeValueModels.Any())
            {
                int order = 0;
                foreach (var valueModel in attributeAddModel.AttributeValueModels)
                {
                    attribute?.AttributeValues?.Add(new AttributeValue
                    {
                        Id = Guid.NewGuid(),
                        Value = valueModel.Value,
                        IntOrder = order++,
                        CreationDate = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.AttributeRepository.AddAsync(attribute);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
            };
        }

        public async Task<ResponseModel> GetAll(AttributeFilterModel attributeFilterModel)
        {
            var attributeLists = await _unitOfWork.AttributeRepository.GetAllAsync(
                filter: _ => _.IsDeleted == attributeFilterModel.IsDeleted,
                include: _ => _.Include(_ => _.AttributeValues),
                pageIndex: attributeFilterModel.PageIndex,
                pageSize: attributeFilterModel.PageSize
                );
            var attributeModels = attributeLists.Data.Select(_ => new AttributeModel
            {
                Id = _.Id,
                Name = _.Name,
                IsRequired = _.IsRequired,
                Type = _.Type,
            }).ToList();
            var result = new Pagination<AttributeModel>(attributeModels, attributeFilterModel.PageIndex,
                attributeFilterModel.PageSize, attributeLists.TotalCount);

            return new ResponseModel
            {
                Message = "Attributes retrieved successfully",
                Data = result
            };
        }

        public async Task<ResponseModel> GetAttributeValueById(Guid id)
        {
            var attributeValueLists = await _unitOfWork.AttributeValueRepository.GetAllAsync(
                filter: _ => _.IsDeleted == false && _.AttributeId == id
                );
            if (!attributeValueLists.Data.Any())
            {
                return new ResponseModel
                {
                    Message = "Empty",
                    Data = null
                };
            }
            return new ResponseModel
            {
                Message = "AttributeValues retrieved successfully",
                Data = attributeValueLists.Data
            };
        }
    }
}
