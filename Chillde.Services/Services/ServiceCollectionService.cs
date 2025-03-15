using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.ServiceCollectionModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceCollectionModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Services.Services
{
    public class ServiceCollectionService : IServiceCollectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IBadWordFilterService _badWordFilterService;

        public ServiceCollectionService(IUnitOfWork unitOfWork, 
            IClaimService claimService, 
            ICloudinaryHelper cloudinaryHelper, 
            IBadWordFilterService badWordFilterService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _badWordFilterService = badWordFilterService;
        }
        public async Task<ResponseModel> AddAsync(ServiceCollectionAddModel model, string sourceLanguageCode)
        {
            string[] fieldsToCheck = { model.Name! };

            foreach (var field in fieldsToCheck)
            {
                ResponseModel response = sourceLanguageCode == "vi"
                    ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                    : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                if (response.Code != StatusCodes.Status200OK)
                    return response;
            }

            var currentUserId = _claimService.GetCurrentUserId;
            if (string.IsNullOrEmpty(model.Name))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Name is required."
                };
            }

            var serviceCollection = new ServiceCollection
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                ImageUrl = model.ImageUrl == null
                        ? null
                        : await _cloudinaryHelper.UploadImageAsync(
                          model.ImageUrl,
                          "collections",
                          Guid.NewGuid().ToString(),
                          folderName: FolderAttachment.COLLECTION
                         ),
                CreatedById = currentUserId!.Value,
            };

            await _unitOfWork.ServiceCollectionRepository.AddAsync(serviceCollection);
            await _unitOfWork.SaveChangeAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Service collection added successfully.",
            };
        }
        public async Task<ResponseModel> GetAllAsync(ServiceCollectionFilterModel filterModel)
        {
            var serviceCollections = await _unitOfWork.ServiceCollectionRepository.GetAllAsync(
                      sc => sc.IsDeleted == filterModel.IsDeleted &&
                      (string.IsNullOrEmpty(filterModel.Search) || sc.Name!.ToLower().Contains(filterModel.Search.ToLower())),
            requests =>
            {
                switch (filterModel.Order.ToLower())
                {
                    case "creationDate":
                        return filterModel.OrderByDescending
                            ? requests.OrderByDescending(collection => collection.CreationDate)
                            : requests.OrderBy(collection => collection.CreationDate);
                    default:
                        return filterModel.OrderByDescending
                            ? requests.OrderByDescending(collection => collection.CreationDate)
                            : requests.OrderBy(collection => collection.CreationDate);
                }
            },
                    include: collections => collections
                            .Include(sc => sc.ServiceWishlists)
                            .Include(sc => sc.CreatedBy),
                      pageIndex: filterModel.PageIndex,
                      pageSize: filterModel.PageSize);


            var serviceCollectionModels = serviceCollections.Data.Select(sc => new ServiceCollectionModel
            {
                Id = sc.Id,
                Name = sc.Name,
                ImageUrl = sc.ImageUrl,
                CreatedBy = new AccountLiteModel
                {
                    FirstName = sc.CreatedBy!.FirstName,
                    LastName = sc.CreatedBy.LastName,
                    Username = sc.CreatedBy.Username,
                    Email = sc.CreatedBy.Email,
                    Image = sc.CreatedBy.Image
                }
            }).ToList();

            return new ResponseModel
            {
                Data = serviceCollectionModels,
                Message = "Service collections retrieved successfully",
                Code = StatusCodes.Status200OK
            };
        }


        public async Task<ResponseModel> GetByIdAsync(Guid id)
        {
            var serviceCollection = await _unitOfWork.ServiceCollectionRepository.GetAsync(
                id: id,
                include: query => query.Where(sc => sc.Id == id).Include(sc => sc.ServiceWishlists).Include(sc => sc.CreatedBy));

            if (serviceCollection == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service collection not found."
                };
            }
            var serviceCollectionModel = new ServiceCollectionModel
            {
                Id = serviceCollection.Id,
                Name = serviceCollection.Name,
                ImageUrl = serviceCollection.ImageUrl,
                CreatedBy = new AccountLiteModel
                {
                    FirstName = serviceCollection.CreatedBy!.FirstName,
                    LastName = serviceCollection.CreatedBy.LastName,
                    Username = serviceCollection.CreatedBy.Username,
                    Email = serviceCollection.CreatedBy.Email,
                    Image = serviceCollection.CreatedBy.Image
                }
            };
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service collection retrieved successfully.",
                Data = serviceCollection
            };
        }
        public async Task<ResponseModel> UpdateAsync(Guid id, ServiceCollectionAddModel model, string sourceLanguageCode)
        {
            string[] fieldsToCheck = { model.Name! };

            foreach (var field in fieldsToCheck)
            {
                ResponseModel response = sourceLanguageCode == "vi"
                    ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                    : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                if (response.Code != StatusCodes.Status200OK)
                    return response;
            }

            var serviceCollection = await _unitOfWork.ServiceCollectionRepository.GetAsync(id);
            if (serviceCollection == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service collection not found."
                };
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Name is required."
                };
            }
            serviceCollection.Name = model.Name;
            if (model.ImageUrl != null)
            {
                serviceCollection.ImageUrl = await _cloudinaryHelper.UploadImageAsync(
                    model.ImageUrl,
                    "collections",
                    id.ToString(),
                    folderName: FolderAttachment.COLLECTION
                );
            }
            _unitOfWork.ServiceCollectionRepository.Update(serviceCollection);
            await _unitOfWork.SaveChangeAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service collection updated successfully.",
            };
        }

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            var serviceCollection = await _unitOfWork.ServiceCollectionRepository.GetAsync(id);

            if (serviceCollection == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service collection not found."
                };
            }
            _unitOfWork.ServiceCollectionRepository.SoftRemove(serviceCollection);
            await _unitOfWork.SaveChangeAsync();
            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service collection deleted successfully."
            };
        }
    }
}
