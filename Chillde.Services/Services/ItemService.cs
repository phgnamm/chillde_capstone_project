using AutoMapper;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ItemModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IMapper _mapper;

        public ItemService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
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
                SubCategoryName = entity.SubCategory?.Name,
                CategoryId = entity.SubCategory?.CategoryId,
                CategoryName = entity.SubCategory?.Category?.Name
            };

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Entity retrieved successfully.",
                Data = entityModel
            };
        }


    }
}
