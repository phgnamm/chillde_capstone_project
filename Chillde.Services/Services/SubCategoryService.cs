using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ItemModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IMapper _mapper;

        public SubCategoryService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
        }

        public async Task<ResponseModel> AddItem(Guid subcategoryId, ItemAddRangeModel itemAddRangeModel)
        {
            var subCategoryExists = await _unitOfWork.SubCategoryRepository.GetAsync(subcategoryId);
            if (subCategoryExists == null || subCategoryExists.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "SubCategory not found."
                };
            }

            var newItems = new List<ItemModel>();

            if (itemAddRangeModel.ImageUrls != null &&
                itemAddRangeModel.ImageUrls.Count != itemAddRangeModel.ItemAddRequestModels.Count)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "The number of images must match the number of items."
                };
            }
            var itemsToAdd = new List<Item>();
            for (int i = 0; i < itemAddRangeModel.ItemAddRequestModels.Count; i++)
            {
                var requestModel = itemAddRangeModel.ItemAddRequestModels[i];
                string? imageUrl = null;

                if (itemAddRangeModel.ImageUrls != null && itemAddRangeModel.ImageUrls.ElementAtOrDefault(i) != null)
                {
                    imageUrl = await _cloudinaryHelper.UploadImageAsync(
                        itemAddRangeModel.ImageUrls[i],
                        "items",
                        Guid.NewGuid().ToString()
                    );
                }

                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Name = requestModel.Name,
                    Code = string.IsNullOrEmpty(requestModel.Code)
                        ? GenerateSlug(requestModel.Name)
                        : GenerateSlug(requestModel.Code),
                    ImageUrl = imageUrl,
                    SubCategoryId = subcategoryId
                };
                itemsToAdd.Add(item);
                newItems.Add(_mapper.Map<ItemModel>(item));
            }
            await _unitOfWork.ItemRepository.AddRangeAsync(itemsToAdd);
            await _unitOfWork.SaveChangeAsync();


            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Items added successfully.",
                Data = newItems
            };
        }

        public async Task<ResponseModel> GetItemBySubCategory(Guid subcategoryId, ItemFilterModel itemFilterModel)
        {
            var subCategoryExists = await _unitOfWork.SubCategoryRepository.GetAsync(subcategoryId);
            if (subCategoryExists == null || subCategoryExists.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "SubCategory not found."
                };
            }
            Expression<Func<Item, bool>> filter = item =>
                   item.SubCategoryId == subcategoryId &&
                   item.IsDeleted == itemFilterModel.IsDeleted &&
                   (string.IsNullOrEmpty(itemFilterModel.Search) ||
                   item.Name.Contains(itemFilterModel.Search) ||
                   item.Code.Contains(itemFilterModel.Search));


            var items = await _unitOfWork.ItemRepository.GetAllAsync(
                filter: filter,
                include: null
            );

            var itemModel = _mapper.Map<List<ItemModel>>(items.Data);
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Items retrieved successfully.",
                Data = itemModel
            };
        }

        public async Task<ResponseModel> Update(Guid id, SubCategoryUpdateModel subCategoryUpdateModel)
        {
            var subcategory = await _unitOfWork.SubCategoryRepository.GetAsync(id);
            if (subcategory == null || subcategory.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "SubCategory not found."
                };
            }

            if (!string.IsNullOrEmpty(subCategoryUpdateModel.Name))
            {
                var existingCategory = await _unitOfWork.SubCategoryRepository.GetFirstOrDefaultAsync(
                    c => c.Name == subCategoryUpdateModel.Name && c.Id != id && !c.IsDeleted
                );
                if (existingCategory != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "SubCategory code already exists."
                    };
                }
            }

            if (subCategoryUpdateModel.ImageUrl != null)
            {
                var imageUrl = await _cloudinaryHelper.UploadImageAsync(
                    subCategoryUpdateModel.ImageUrl,
                    "subcategories",
                    id.ToString()
                );
                subcategory.ImageUrl = imageUrl;
            }

            subcategory.Name = subCategoryUpdateModel.Name;
            subcategory.Code = string.IsNullOrEmpty(subCategoryUpdateModel.Code)
                ? GenerateSlug(subCategoryUpdateModel.Name)
                : GenerateSlug(subCategoryUpdateModel.Code);

            _unitOfWork.SubCategoryRepository.Update(subcategory);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "SubCategory updated successfully."
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
        private bool IsValidSlug(string code)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-z0-9-_]+$");
        }
    }
}
