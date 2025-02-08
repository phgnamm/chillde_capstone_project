using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Chillde.Services.Services
{
    public class CategoryService : ICategoryServive
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, ICloudinaryHelper cloudinaryHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
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
                    Guid.NewGuid().ToString(),
                    folderName: FolderAttachment.CATEGORY
                );
            }
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

        public async Task<ResponseModel> AddList(CategoryAddRangeModel categoryAddRangeModel)
        {
            if (categoryAddRangeModel.CategoryAddRequestModels.Count != categoryAddRangeModel.ImageUrls!.Count)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "The number of categories and images must match."
                };
            }

            var newCategories = new List<Category>();

            for (int i = 0; i < categoryAddRangeModel.CategoryAddRequestModels.Count; i++)
            {
                var categoryModel = categoryAddRangeModel.CategoryAddRequestModels[i];
                var imageFile = categoryAddRangeModel.ImageUrls[i];

                var existingCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(s => s.Name == categoryModel.Name);
                if (existingCategory != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Category with name '{categoryModel.Name}' already exists."
                    };
                }

                string code = string.IsNullOrWhiteSpace(categoryModel.Code)
                    ? GenerateSlug(categoryModel.Name)
                    : categoryModel.Code;

                if (!IsValidSlug(code))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Invalid format for Code in category '{categoryModel.Name}'. Use only lowercase letters, numbers, hyphens, or underscores."
                    };
                }

                string? imageUrl = null;
                if (imageFile!= null)
                {
                    imageUrl = await _cloudinaryHelper.UploadImageAsync(
                        imageFile,
                        "categories",
                        Guid.NewGuid().ToString()
                    );
                }

                newCategories.Add(new Category
                {
                    Name = categoryModel.Name,
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

        public async Task<ResponseModel> AddSubcategory(Guid categoryId, SubCategoryAddRangeModel subCategoryAddRangeModel)
        {
            var categoryExists = await _unitOfWork.CategoryRepository.GetAsync(categoryId);
            if (categoryExists == null || categoryExists.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Category not found."
                };
            }

            var newSubCategories = new List<SubCategoryModel>();

            if (subCategoryAddRangeModel.ImageUrls != null &&
                subCategoryAddRangeModel.ImageUrls.Count != subCategoryAddRangeModel.SubCategoryAddRequestModels.Count)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "The number of images must match the number of subcategories."
                };
            }
            var subCategoriesToAdd = new List<SubCategory>();
            for (int i = 0; i < subCategoryAddRangeModel.SubCategoryAddRequestModels.Count; i++)
            {
                var requestModel = subCategoryAddRangeModel.SubCategoryAddRequestModels[i];
                string? imageUrl = null;

                if (subCategoryAddRangeModel.ImageUrls != null && subCategoryAddRangeModel.ImageUrls.ElementAtOrDefault(i) != null)
                {
                    imageUrl = await _cloudinaryHelper.UploadImageAsync(
                        subCategoryAddRangeModel.ImageUrls[i],
                        "subcategories",
                        Guid.NewGuid().ToString()
                    );
                }

                var subCategory = new SubCategory
                {
                    Id = Guid.NewGuid(),
                    Name = requestModel.Name,
                    Code = string.IsNullOrEmpty(requestModel.Code)
                        ? GenerateSlug(requestModel.Name)
                        : GenerateSlug(requestModel.Code),
                    ImageUrl = imageUrl,
                    CategoryId = categoryId
                };
                subCategoriesToAdd.Add(subCategory);
                newSubCategories.Add(_mapper.Map<SubCategoryModel>(subCategory));
            }
            await _unitOfWork.SubCategoryRepository.AddRangeAsync(subCategoriesToAdd);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Subcategories added successfully.",
                Data = newSubCategories
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
                    category.Name!.Contains(categoryFilterModel.Search) ||
                    category.Code!.Contains(categoryFilterModel.Search));

            Func<IQueryable<Category>, IQueryable<Category>> include = categories =>
                     categories.Include(c => c.SubCategories);

            var categorys = await _unitOfWork.CategoryRepository.GetAllAsync(
                            filter: filter,
                            include: include,
                            pageIndex: categoryFilterModel.PageIndex,
            pageSize: categoryFilterModel.PageSize
            );
            /* var cateroryModels = categorys.Data.Select(_ => new CateroryModel
             {
                 Id = _.Id,
                 Name = _.Name,
                 Code = _.Code,      
                 ImageUrl = _.ImageUrl,
             }).ToList();*/
            var cateroryModels = _mapper.Map<List<CategoryModel>>(categorys.Data);

            var result = new Pagination<CategoryModel>(cateroryModels, categoryFilterModel.PageIndex,
              categoryFilterModel.PageSize, categorys.TotalCount);

            return new ResponseModel
            {
                Message = "Get all categorys successfully",
                Data = result
            };



        }

        public async Task<ResponseModel> GetById(Guid id)
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

            var cateroryModels = _mapper.Map<CategoryModel>(category);

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Category retrieved successfully.",
                Data = cateroryModels
            };
        }

        public async Task<ResponseModel> GetSubcategoriesByCategory(Guid categoryId, SubCategoryFilterModel subCategoryFilterModel)
        {
            var categoryExists = await _unitOfWork.CategoryRepository.GetAsync(categoryId);
            if (categoryExists == null || categoryExists.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Category not found."
                };
            }
            Expression<Func<SubCategory, bool>> filter = subcategory =>
                   subcategory.CategoryId == categoryId &&
                   subcategory.IsDeleted == subCategoryFilterModel.IsDeleted &&
                   (string.IsNullOrEmpty(subCategoryFilterModel.Search) ||
                   subcategory.Name!.Contains(subCategoryFilterModel.Search) ||
                   subcategory.Code!.Contains(subCategoryFilterModel.Search));


            var subcategories = await _unitOfWork.SubCategoryRepository.GetAllAsync(
                filter: filter,
                include: null
            );

            var subcategoriesModel = _mapper.Map<List<SubCategoryModel>>(subcategories.Data);
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Subcategories retrieved successfully.",
                Data = subcategoriesModel
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
                        Message = "Category name already exists."
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
                ? GenerateSlug(categoryUpdateModel.Name!)
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
            return Regex.IsMatch(code, @"^[a-z0-9-_]+$");
        }
    }
}