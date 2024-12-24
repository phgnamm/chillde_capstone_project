using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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
