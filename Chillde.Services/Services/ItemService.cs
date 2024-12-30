using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AttributeModels;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ItemModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Chillde.Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public ItemService(IUnitOfWork unitOfWork, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> GetAllAttributesById(Guid itemId)
        {
            var attributeLists = await _unitOfWork.ItemAttributeRepository.GetAllAsync(
                filter: _ => _.ItemId == itemId && _.IsDeleted == false,
                include: _ => _.Include(_ => _.Attribute)
                );
            var attributeModels = attributeLists.Data.Select(_ => new AttributeModel 
            {
                Id = _.Attribute.Id,
                Name = _.Attribute.Name,
                Type = _.Attribute.Type,
            }).ToList();
            return new ResponseModel
            {
                Data = attributeModels,
                Message = "Attributes retrieved successfully"
            };
        }

        public async Task<ResponseModel> GetById(Guid itemId)
        {
            var entity = await _unitOfWork.ItemRepository.GetAsync(
                id: itemId,
                include: query => query.Where(e => e.Id == itemId).Include(e => e.SubCategory).ThenInclude(sc => sc.Category)
            );

            if (entity == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Entity not found."
                };
            }

            var entityModel = new ItemModelById
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code,
                ImageUrl = entity.ImageUrl,
                SubCategoryId = entity.SubCategoryId,
                SubCategoryName = entity.SubCategory.Name,
                CategoryId = entity.SubCategory.CategoryId,
                CategoryName = entity.SubCategory.Category.Name
            };

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Entity retrieved successfully.",
                Data = entityModel
            };
        }
        public async Task<ResponseModel> Update(Guid id, ItemUpdateModel itemUpdateModel)
        {
            var item = await _unitOfWork.ItemRepository.GetAsync(id);
            if (item == null || item.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Item not found."
                };
            }

            if (!string.IsNullOrEmpty(itemUpdateModel.Name))
            {
                var existingItem = await _unitOfWork.ItemRepository.GetFirstOrDefaultAsync(
                    c => c.Name == itemUpdateModel.Name && c.Id != id && !c.IsDeleted
                );
                if (existingItem != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "Item name already exists."
                    };
                }
            }

            if (itemUpdateModel.ImageUrl != null)
            {
                var imageUrl = await _cloudinaryHelper.UploadImageAsync(
                    itemUpdateModel.ImageUrl,
                    "items",
                    id.ToString()
                );
                item.ImageUrl = imageUrl;
            }

            item.Name = itemUpdateModel.Name;
            item.Code = string.IsNullOrEmpty(itemUpdateModel.Code)
                ? GenerateSlug(itemUpdateModel.Name!)
                : GenerateSlug(itemUpdateModel.Code);

            _unitOfWork.ItemRepository.Update(item);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Item updated successfully."
            };
        }
        private string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Replace("&", "-and-");

            input = input.Replace(",", "-");

            input = input.ToLowerInvariant();

            input = Regex.Replace(input, @"[^a-z0-9\s-]", string.Empty);

            input = Regex.Replace(input, @"\s+", "-");

            input = input.Trim('-');

            return input;
        }
        /*private bool IsValidSlug(string code)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-z0-9-_]+$");
        }*/

    }
}
