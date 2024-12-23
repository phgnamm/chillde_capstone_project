using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class CategoryService : ICategoryServive
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public CategoryService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> Add(CategoryAddModel categoryAddModel)
        {
            var existingCategoty = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(c => c.Name == categoryAddModel.Name);
            if (existingCategoty is not null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "A category with the same name already exists."
                };
            }
            string code;
            if (string.IsNullOrWhiteSpace(categoryAddModel.Code))
            {
                code = GenerateSlug(categoryAddModel.Name);
            }
            else
            {
                if (!IsValidSlug(categoryAddModel.Code))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid format for Code. Use only lowercase letters, numbers, hyphens, or underscores."
                    };
                }

                code = categoryAddModel.Code;
            }

            string? imageUrl = null;
            if (categoryAddModel.ImageUrl != null)
            {
                imageUrl = await _cloudinaryHelper.UploadImageAsync(
                    categoryAddModel.ImageUrl,
                    "categories",
                    Guid.NewGuid().ToString()
                );
            }

            string generatedCode = GenerateSlug(categoryAddModel.Name);

            var newCategory = new Category
            {
                Name = categoryAddModel.Name,
                Code = code,
                ImageUrl = imageUrl
            };

            await _unitOfWork.CategoryRepository.AddAsync(newCategory);
            await _unitOfWork.SaveChangeAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Category created successfully.",
                Data = newCategory
            };
        }

        public async Task<ResponseModel> AddList(List<CategoryAddModel> categoryAddModels)
        {
            var newCategories = new List<Category>();

            foreach (var categoryAddModel in categoryAddModels)
            {
                var existingCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(c => c.Name == categoryAddModel.Name);

                if (existingCategory != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Category with name '{categoryAddModel.Name}' already exists."
                    };
                }

                string code;
                if (string.IsNullOrWhiteSpace(categoryAddModel.Code))
                {
                    code = GenerateSlug(categoryAddModel.Name);
                }
                else
                {
                    if (!IsValidSlug(categoryAddModel.Code))
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Invalid format for Code in category '{categoryAddModel.Name}'. Use only lowercase letters, numbers, hyphens, or underscores."
                        };
                    }
                    code = categoryAddModel.Code;
                }

                string? imageUrl = null;
                if (categoryAddModel.ImageUrl != null)
                {
                    imageUrl = await _cloudinaryHelper.UploadImageAsync(
                        categoryAddModel.ImageUrl,
                        "categories",
                        Guid.NewGuid().ToString()
                    );
                }

                newCategories.Add(new Category
                {
                    Name = categoryAddModel.Name,
                    Code = code,
                    ImageUrl = imageUrl
                });
            }

            await _unitOfWork.CategoryRepository.AddRangeAsync(newCategories);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Categories added successfully.",
                Data = newCategories
            };
        }

        public async Task<ResponseModel> Delete(Guid id)
        {
            var category = await _unitOfWork.CategoryRepository.GetAsync(id);
            if (category == null || category.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Category not found."
                };
            }         
            _unitOfWork.CategoryRepository.HardRemove(category);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Category deleted successfully."
            };
        }

        public async Task<ResponseModel> GetAll(CategoryFilterModel categoryFilterModel)
        {
            Expression<Func<Category, bool>> filter = category =>
                    category.IsDeleted == categoryFilterModel.IsDeleted &&
                    (string.IsNullOrEmpty(categoryFilterModel.Search) ||
                    category.Name.Contains(categoryFilterModel.Search) ||
                    category.Code.Contains(categoryFilterModel.Search));

            Func<IQueryable<Category>, IQueryable<Category>> include = categories =>
                     categories.Include(c => c.SubCategories);

            var categorys = await _unitOfWork.CategoryRepository.GetAllAsync(
                            filter: filter,
                            include: include,
                            pageIndex: categoryFilterModel.PageIndex,
            pageSize: categoryFilterModel.PageSize
            );

            var result = new Pagination<Category>(categorys.Data, categoryFilterModel.PageIndex,
              categoryFilterModel.PageSize, categorys.TotalCount);

            return new ResponseModel
            {
                Message = "Get all categorys successfully",
                Data = result
            };



        }

        public async Task<ResponseModel> Update(Guid id, CategoryUpdateModel categoryUpdateModel)
        {
            var category = await _unitOfWork.CategoryRepository.GetAsync(id);
            if (category == null || category.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Category not found."
                };
            }

            if (!string.IsNullOrEmpty(categoryUpdateModel.Name))
            {
                var existingCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(
                    c => c.Name == categoryUpdateModel.Name && c.Id != id && !c.IsDeleted
                );
                if (existingCategory != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "Category code already exists."
                    };
                }
            }

            if (categoryUpdateModel.ImageUrl != null)
            {
                var imageUrl = await _cloudinaryHelper.UploadImageAsync(
                    categoryUpdateModel.ImageUrl,
                    "categories",
                    id.ToString()
                );
                category.ImageUrl = imageUrl;
            }

            category.Name = categoryUpdateModel.Name;
            category.Code = string.IsNullOrEmpty(categoryUpdateModel.Code)
                ? GenerateSlug(categoryUpdateModel.Name)
                : GenerateSlug(categoryUpdateModel.Code);

            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Category updated successfully."
            };
        }

        private string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            input = input.Replace("&", "-and-", StringComparison.OrdinalIgnoreCase);

            input = input.ToLowerInvariant();

            input = Regex.Replace(input, @"[^a-z0-9\s-]", "");

            input = Regex.Replace(input, @"[\s-]+", "-").Trim('-');

            return input;
        }
        private bool IsValidSlug(string code)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-z0-9-_]+$");
        }
    }
}