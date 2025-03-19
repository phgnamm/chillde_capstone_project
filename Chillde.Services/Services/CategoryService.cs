using System.Linq.Expressions;
using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.CategoriesModels;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Chillde.Repositories.Common;
using Chillde.Services.Common;

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
            try
            {
                var existingCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == categoryAddModel.Name.ToLower());
                if (existingCategory != null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = $"Category with name '{categoryAddModel.Name}' already exists."
                    };
                }
                // Handle parent category if specified
                if (categoryAddModel.ParentId.HasValue)
                {
                    var parentCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(c => c.Id == categoryAddModel.ParentId);
                    if (parentCategory == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Parent category with id '{categoryAddModel.ParentId}' does not exist."
                        };
                    }
                }
                // Handle image upload if specified 
                string? imageUrl = null;
                if (categoryAddModel.AttachmentUrl != null)
                {
                    try
                    {
                        imageUrl = await _cloudinaryHelper.UploadImageAsync(
                           categoryAddModel.AttachmentUrl,
                           "categories",
                           Guid.NewGuid().ToString(),
                           folderName: FolderAttachment.CATEGORY
                       );
                    }
                    catch (Exception e)
                    {
                        return new ResponseModel()
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = e.Message
                        };

                    }
                }
                // Create new category
                var newCategory = new Category
                {
                    Name = categoryAddModel.Name,
                    AttachmentUrl = imageUrl,
                    ParentId = categoryAddModel.ParentId,
                    AttachmentAlt = categoryAddModel.AttachmentAlt
                };
                // Add new category to database
                await _unitOfWork.CategoryRepository.AddAsync(newCategory);
                await _unitOfWork.SaveChangeAsync();

                var responseCategory = _mapper.Map<CategoryModel>(newCategory);
                return new ResponseModel()
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Category created successfully",
                    Data = responseCategory
                };

            }
            catch (Exception e)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = e.Message
                };

            }

        }

        public async Task<ResponseModel> AddList(CategoryAddRangeModel categoryAddRangeModel)
        {
            if (categoryAddRangeModel.CategoryAddRequestModels == null ||
                !categoryAddRangeModel.CategoryAddRequestModels.Any())
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "No categories provided for creation."
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (categoryAddRangeModel.ParentId.HasValue)
                {
                    var parentCategory = await _unitOfWork.CategoryRepository
                        .GetFirstOrDefaultAsync(c => c.Id == categoryAddRangeModel.ParentId);
                    if (parentCategory == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Parent category with id '{categoryAddRangeModel.ParentId}' does not exist."
                        };
                    }
                }

                var newCategories = new List<Category>();
                foreach (var categoryModel in categoryAddRangeModel.CategoryAddRequestModels)
                {
                    if (string.IsNullOrWhiteSpace(categoryModel.Name))
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Category name is required."
                        };
                    }

                    var existingCategory = await _unitOfWork.CategoryRepository
                        .GetFirstOrDefaultAsync(s => s.Name != null &&
                            s.Name.ToLower() == categoryModel.Name.ToLower());
                    if (existingCategory != null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Category with name '{categoryModel.Name}' already exists."
                        };
                    }

                    string? imageUrl = null;
                    if (categoryModel.AttachmentUrl != null)
                    {
                        try
                        {
                            imageUrl = await _cloudinaryHelper.UploadImageAsync(
                                categoryModel.AttachmentUrl,
                                "categories",
                                Guid.NewGuid().ToString(),
                                folderName: FolderAttachment.CATEGORY
                            );
                        }
                        catch (Exception e)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return new ResponseModel
                            {
                                Code = StatusCodes.Status500InternalServerError,
                                Message = $"Failed to upload image for category '{categoryModel.Name}': {e.Message}"
                            };
                        }
                    }

                    var newCategory = new Category
                    {
                        Name = categoryModel.Name,
                        AttachmentUrl = imageUrl,
                        ParentId = categoryAddRangeModel.ParentId,
                        AttachmentAlt = categoryModel.AttachmentAlt
                    };
                    newCategories.Add(newCategory);
                }

                await _unitOfWork.CategoryRepository.AddRangeAsync(newCategories);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                var responseCategories = _mapper.Map<List<CategoryModel>>(newCategories);
                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Categories added successfully.",
                    Data = responseCategories
                };
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred: {e.Message}"
                };
            }
        }

        public async Task<ResponseModel> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid category ID."
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = await _unitOfWork.CategoryRepository.GetAsync(id);
                if (category == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Category with ID {id} not found."
                    };
                }

                var serviceRelated = await _unitOfWork.ServiceRepository.GetAllAsync(c => c.CategoryId == id);
                if (serviceRelated.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot delete category because it is related to ServiceRepository entities."
                    };
                }

                var repoRelatedService = await _unitOfWork.ServiceRepository.GetAllAsync(c => c.CategoryId == id);
                var repoRelatedRequest = await _unitOfWork.RequestRepository.GetAllAsync(c => c.CategoryId == id);

                if (repoRelatedService.Data.Any() || repoRelatedRequest.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot delete category because it is related to Repository entities."
                    };
                }

                var children = await _unitOfWork.CategoryRepository.GetAllAsync(
                    filter: c => c.ParentId == category.Id
                );
                if (children.Data.Any())
                {
                    var updatedChildren = children.Data.Select(c => { c.ParentId = null; return c; }).ToList();
                    _unitOfWork.CategoryRepository.UpdateRange(updatedChildren);
                    await _unitOfWork.SaveChangeAsync();
                }

                _unitOfWork.CategoryRepository.HardRemove(category);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = $"Category with ID {id} deleted successfully. Children updated to have no parent."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while deleting category: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> GetAll(CategoryFilterModel categoryFilterModel)
        {
            try
            {
                Expression<Func<Category, bool>> filter = category =>
                    (category.IsDeleted == categoryFilterModel.IsDeleted) &&
                    (string.IsNullOrEmpty(categoryFilterModel.Search) ||
                     (category.Name != null && category.Name.ToLower().Contains(categoryFilterModel.Search.ToLower())) ||
                     (category.Slug != null && category.Slug.ToLower().Contains(categoryFilterModel.Search.ToLower()))) &&
                    (string.IsNullOrEmpty(categoryFilterModel.Slug) || (category.Slug != null && category.Slug == categoryFilterModel.Slug)) &&
                    (!categoryFilterModel.ParentId.HasValue || category.ParentId == categoryFilterModel.ParentId);

                var allCategoriesResult = await _unitOfWork.CategoryRepository.GetAllAsync(filter: filter);
                var allCategories = allCategoriesResult.Data;

                if (categoryFilterModel.IncludeChildren)
                {
                    var rootCategories = allCategories
                        .Where(c => c.ParentId == categoryFilterModel.ParentId)
                        .Select(c => BuildCategoryTree(c, allCategories))
                        .ToList();

                    var totalCount = rootCategories.Count;
                    var pagedRootCategories = rootCategories
                        .Skip((categoryFilterModel.PageIndex - 1) * categoryFilterModel.PageSize)
                        .Take(categoryFilterModel.PageSize)
                        .ToList();

                    var result = new Pagination<CategoryTreeModel>(
                        pagedRootCategories,
                        categoryFilterModel.PageIndex,
                        categoryFilterModel.PageSize,
                        totalCount
                    );

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Get all categories with tree successfully",
                        Data = result
                    };
                }
                else
                {
                    var flatCategories = allCategories
                        .Where(c => c.ParentId == categoryFilterModel.ParentId)
                        .Select(category => new CategoryTreeModel
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Slug = category.Slug,
                            ParentId = category.ParentId,
                            AttachmentUrl = category.AttachmentUrl,
                            AttachmentAlt = category.AttachmentAlt,
                            CreatedById = category.CreatedById,
                            CreationDate = category.CreationDate,
                            ModificationDate = category.ModificationDate,
                            ModifiedById = category.ModifiedById,
                            IsDeleted = category.IsDeleted
                        })
                        .ToList();

                    var totalCount = flatCategories.Count;
                    var pagedFlatCategories = flatCategories
                        .Skip((categoryFilterModel.PageIndex - 1) * categoryFilterModel.PageSize)
                        .Take(categoryFilterModel.PageSize)
                        .ToList();

                    var result = new Pagination<CategoryTreeModel>(
                        pagedFlatCategories,
                        categoryFilterModel.PageIndex,
                        categoryFilterModel.PageSize,
                        totalCount
                    );

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Get all categories as flat list successfully",
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while retrieving categories: {ex.Message}"
                };
            }

        }

        private CategoryTreeModel BuildCategoryTree(Category category, IEnumerable<Category> allCategories)
        {
            var treeModel = new CategoryTreeModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = category.ParentId,
                AttachmentUrl = category.AttachmentUrl,
                AttachmentAlt = category.AttachmentAlt,
                CreatedById = category.CreatedById,
                CreationDate = category.CreationDate,
                ModificationDate = category.ModificationDate,
                ModifiedById = category.ModifiedById,
                IsDeleted = category.IsDeleted
            };

            var categories = allCategories.ToList();
            var children = categories.Where(c => c.ParentId == category.Id).ToList();
            treeModel.Children = children.Select(c => BuildCategoryTree(c, categories)).ToList();

            return treeModel;
        }
        public async Task<ResponseModel> Update(Guid id, CategoryUpdateModel categoryUpdateModel)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid category ID."
                    };
                }

                var category = await _unitOfWork.CategoryRepository.GetAsync(id);
                if (category == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Category with ID {id} not found."
                    };
                }

                var serviceRelated = await _unitOfWork.ServiceRepository.GetAllAsync(c => c.CategoryId == id);
                if (serviceRelated.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot update category because it is being used by ServiceRepository entities."
                    };
                }

                var repoRelatedService = await _unitOfWork.ServiceRepository.GetAllAsync(c => c.CategoryId == id);
                var repoRelatedRequest = await _unitOfWork.RequestRepository.GetAllAsync(c => c.CategoryId == id);

                if (repoRelatedService.Data.Any() || repoRelatedRequest.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot delete category because it is related to Repository entities."
                    };
                }

                if (!string.IsNullOrEmpty(categoryUpdateModel.Name) && categoryUpdateModel.Name.ToLower() != category.Name?.ToLower())
                {
                    var existingCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == categoryUpdateModel.Name.ToLower() && s.Id != id);
                    if (existingCategory != null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Category with name '{categoryUpdateModel.Name}' already exists."
                        };
                    }
                }

                if (categoryUpdateModel.ParentId.HasValue)
                {
                    var parentCategory = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(c => c.Id == categoryUpdateModel.ParentId);
                    if (parentCategory == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = $"Parent category with ID '{categoryUpdateModel.ParentId}' does not exist."
                        };
                    }
                    if (categoryUpdateModel.ParentId == id)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "A category cannot be its own parent."
                        };
                    }
                }

                string? imageUrl = category.AttachmentUrl;
                if (categoryUpdateModel.AttachmentUrl != null)
                {
                    try
                    {
                        imageUrl = await _cloudinaryHelper.UploadImageAsync(
                            categoryUpdateModel.AttachmentUrl,
                            "categories",
                            Guid.NewGuid().ToString(),
                            folderName: FolderAttachment.CATEGORY
                        );
                    }
                    catch (Exception e)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = $"Failed to upload image to Cloudinary: {e.Message}"
                        };
                    }
                }

                if (!string.IsNullOrEmpty(categoryUpdateModel.Name))
                {
                    category.Name = categoryUpdateModel.Name;
                }

                category.ParentId = categoryUpdateModel.ParentId;
                category.AttachmentAlt = categoryUpdateModel.AttachmentAlt;
                category.AttachmentUrl = imageUrl;

                _unitOfWork.CategoryRepository.Update(category);
                await _unitOfWork.SaveChangeAsync();

                var responseCategory = _mapper.Map<CategoryModel>(category);
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Category updated successfully",
                    Data = responseCategory
                };
            }
            catch (Exception e)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = e.Message
                };
            }

        }

        public async Task<ResponseModel> GetByIdOrSlug(string idOrSlug)
        {
            try
            {
                if (string.IsNullOrEmpty(idOrSlug))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Id or Slug must be provided."
                    };
                }

                Category? category;

                if (Guid.TryParse(idOrSlug, out Guid categoryId))
                {
                    category = await _unitOfWork.CategoryRepository.GetAsync(categoryId);
                }
                else
                {
                    Expression<Func<Category, bool>> filter = c =>
                        c.Slug == idOrSlug && c.IsDeleted == false;

                    category = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(filter);
                }

                if (category == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Category with Id or Slug '{idOrSlug}' not found."
                    };
                }

                if (category.IsDeleted)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Category with Id or Slug '{idOrSlug}' not found."
                    };
                }

                var categoryModel = new CategoryModel  
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    ParentId = category.ParentId,
                    AttachmentAlt = category.AttachmentAlt,
                    AttachmentUrl = category.AttachmentUrl,
                    CreatedById = category.CreatedById,
                    CreationDate = category.CreationDate,
                    ModificationDate = category.ModificationDate,
                    ModifiedById = category.ModifiedById,
                    DeletionDate = category.DeletionDate,
                    IsDeleted = category.IsDeleted,
                };

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Category retrieved successfully",
                    Data = categoryModel
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while retrieving category: {ex.Message}"
                };
            }
        }
    }

}
