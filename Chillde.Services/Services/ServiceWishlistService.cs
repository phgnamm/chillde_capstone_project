using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceWishlistModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Services.Services
{
    public class ServiceWishlistService : IServiceWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceWishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> AddRangeAsync(Guid serviceCollectionId, List<Guid> serviceIds)
        {
            var serviceCollection = await _unitOfWork.ServiceCollectionRepository.GetAsync(serviceCollectionId);
            if (serviceCollection == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service collection not found."
                };
            }

            var serviceWishlists = serviceIds.Select(serviceId => new ServiceWishlist
            {
                Id = Guid.NewGuid(),
                ServiceCollectionId = serviceCollectionId,
                ServiceId = serviceId
            }).ToList();

            await _unitOfWork.ServiceWishlistRepository.AddRangeAsync(serviceWishlists);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Service wishlists added successfully."
            };
        }


        public async Task<ResponseModel> GetAllAsync(Guid ServiceCollectionId, ServiceWishlistFilterModel filterModel)
        {
            var wishlists = await _unitOfWork.ServiceWishlistRepository.GetAllAsync(
                sw => sw.ServiceCollectionId == ServiceCollectionId,
                requests =>
                {
                    switch (filterModel.Order.ToLower())
                    {
                        case "creationDate":
                            return filterModel.OrderByDescending
                                ? requests.OrderByDescending(wishlist => wishlist.CreationDate)
                                : requests.OrderBy(wishlist => wishlist.CreationDate);
                        default:
                            return filterModel.OrderByDescending
                                ? requests.OrderByDescending(wishlist => wishlist.CreationDate)
                                : requests.OrderBy(wishlist => wishlist.CreationDate);
                    }
                },
                include: wishlist => wishlist.Include(sw => sw.Service),
                pageIndex: filterModel.PageIndex,
                pageSize: filterModel.PageSize);

            var wishlistModels = wishlists.Data.Select(sw => new ServiceWishlistModel
            {
                Id = sw.Id,
                ServiceId = sw.ServiceId,
                ServiceName = sw.Service.Name!,
            }).ToList();

            return new ResponseModel
            {
                Data = wishlistModels,
                Message = "Service wishlists retrieved successfully",
                Code = StatusCodes.Status200OK
            };
        }



        public async Task<ResponseModel> GetByIdAsync(Guid id)
        {
            var serviceWishlist = await _unitOfWork.ServiceWishlistRepository.GetAsync(
                id: id,
                include: sw => sw.Include(sw => sw.Service));

            if (serviceWishlist == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service wishlist not found."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service wishlist retrieved successfully.",
                Data = new
                {
                    serviceWishlist.Id,
                    serviceWishlist.ServiceId,
                    ServiceName = serviceWishlist.Service.Name
                }
            };
        }

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            var serviceWishlist = await _unitOfWork.ServiceWishlistRepository.GetAsync(id);

            if (serviceWishlist == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service wishlist not found."
                };
            }

            _unitOfWork.ServiceWishlistRepository.SoftRemove(serviceWishlist);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service wishlist deleted successfully."
            };
        }

        public async Task<ResponseModel> UpdateAsync(Guid id, Guid newServiceId)
        {
            var serviceWishlist = await _unitOfWork.ServiceWishlistRepository.GetAsync(id);

            if (serviceWishlist == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service wishlist not found."
                };
            }

            serviceWishlist.ServiceId = newServiceId;
            _unitOfWork.ServiceWishlistRepository.Update(serviceWishlist);
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status200OK,
                Message = "Service wishlist updated successfully."
            };
        }
    }
}
