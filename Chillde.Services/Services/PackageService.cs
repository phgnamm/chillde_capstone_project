using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PackageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id)
        {
            try
            {
                var package = await _unitOfWork.PackageRepository.GetAsync(id);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                package.Price = packageUpdateModel.Price;

                _unitOfWork.PackageRepository.Update(package);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Package updated successfully.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            try
            {
                var package = await _unitOfWork.PackageRepository.GetAsync(id);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                Expression<Func<Repositories.Entities.Order, bool>> filter = order =>
                         order.PackageId == id &&
                         order.IsDeleted == false;

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                    filter: filter,
                    include: null
                );

                if (orders == null)
                {
                    _unitOfWork.PackageRepository.HardRemove(package);
                }

                _unitOfWork.PackageRepository.SoftRemove(package);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully delete."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                };
            }
        }

            //public async Task<ResponseModel> AddFeatureAsync(PackageAddModel packageAddModel, Guid serviceId)
            //{
            //    try
            //    {
            //        var service = await _unitOfWork.ServiceRepository.GetAsync(serviceId);
            //        if (service == null)
            //        {
            //            return new ResponseModel
            //            {
            //                Code = StatusCodes.Status404NotFound,
            //                Message = "Service not found."
            //            };
            //        }

            //        var numberOfExistedPackage = _unitOfWork.PackageRepository.GetAllPackageFromService(serviceId).Result.Count();
            //        if (numberOfExistedPackage > 3)
            //        {
            //            return new ResponseModel
            //            {
            //                Code = StatusCodes.Status422UnprocessableEntity,
            //                Message = "Number of packages cannot exceed 3."
            //            };
            //        }

            //        var package = new Package
            //        {
            //            Name = packageAddModel.Name,
            //            Description = packageAddModel.Description,
            //            Price = packageAddModel.Price,
            //            ServiceId = serviceId,
            //        };

            //        await _unitOfWork.PackageRepository.AddAsync(package);
            //        await _unitOfWork.SaveChangeAsync();

            //        return new ResponseModel
            //        {
            //            Code = StatusCodes.Status201Created,
            //            Message = "Package successfully created.",
            //            Data = package
            //        };
            //    }
            //    catch (Exception ex)
            //    {
            //        return new ResponseModel
            //        {
            //            Code = StatusCodes.Status500InternalServerError,
            //            Message = ex.Message
            //        };
            //    }
            //}
        }
}
