using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;

namespace Chillde.Services.Services
{
    public class RequestAttributeService //: IRequestAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestAttributeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //    public async Task<ResponseModel> Add(ItemAttributeAddModel itemAttributeAddModel)
        //    {
        //        var newItemAttributes = new List<ItemAttribute>();
        //        foreach (var attribute in itemAttributeAddModel.AttributeIds)
        //        {
        //            var existingAttributeInItem = await _unitOfWork.ItemAttributeRepository.FirstOrDefaultAsync(itemAttributeAddModel.ItemId, attribute);
        //            if (existingAttributeInItem != null) {
        //                return new ResponseModel
        //                {
        //                    Code = StatusCodes.Status400BadRequest,
        //                    Message = $"Attribute '{existingAttributeInItem.Attribute.Name}' has existed in this item"
        //                };
        //            }
        //            var newItemAttribute = new ItemAttribute
        //            {
        //                Id = Guid.NewGuid(),
        //                AttributeId = attribute,
        //                ItemId = itemAttributeAddModel.ItemId,
        //            };
        //            newItemAttributes.Add(newItemAttribute);
        //        }
        //        await _unitOfWork.ItemAttributeRepository.AddRangeAsync(newItemAttributes);
        //        var result = await _unitOfWork.SaveChangeAsync();
        //        return result > 0 ?
        //            new ResponseModel
        //            {
        //                Data = newItemAttributes,
        //                Code = StatusCodes.Status201Created,
        //                Message = "Created successfully"
        //            } :
        //            new ResponseModel
        //            {
        //                Code = StatusCodes.Status400BadRequest,
        //                Message = "Created unsuccessfully"
        //            };
        //    }
        //}
    }
}
