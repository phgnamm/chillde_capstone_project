using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string slug = name.ToLowerInvariant();

            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^\w\s-]", "");

            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

            return slug;
        }
        private bool IsValidSlug(string code)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-z0-9-_]+$");
        }
    }
}