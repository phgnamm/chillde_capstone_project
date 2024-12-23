using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel> Add(PackageAddModel packageAddModel, Guid serviceId)
        {
            try
            {
                var service = await _unitOfWork.ServiceRepository.GetAsync(serviceId);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                var numberOfExistedPackage = _unitOfWork.PackageRepository.GetAllPackageFromService(serviceId).Result.Count();
                if (numberOfExistedPackage > 3)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Number of packages cannot exceed 3."
                    };
                }

                var package = new Package
                {
                    Name = packageAddModel.Name,
                    Description = packageAddModel.Description,
                    Price = packageAddModel.Price,
                    ServiceId = serviceId,
                };

                await _unitOfWork.PackageRepository.AddAsync(package);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Package created successfully.",
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
    }
}
