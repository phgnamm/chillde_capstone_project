using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ItemAttributeModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class ItemAttributeService : IItemAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemAttributeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> Add(ItemAttributeAddModel itemAttributeAddModel)
        {
            var newItemAttributes = new List<ItemAttribute>();
            foreach (var attribute in itemAttributeAddModel.AttributeIds)
            {
                var newItemAttribute = new ItemAttribute
                {
                    Id = Guid.NewGuid(),
                    AttributeId = attribute,
                    ItemId = itemAttributeAddModel.ItemId,
                };
                newItemAttributes.Add(newItemAttribute);
            }
            await _unitOfWork.ItemAttributeRepository.AddRangeAsync(newItemAttributes);
            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0 ?
                new ResponseModel
                {
                    Data = newItemAttributes,
                    Code = StatusCodes.Status201Created,
                    Message = "Created successfully"
                } :
                new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Created unsuccessfully"
                };
        }
    }
}
