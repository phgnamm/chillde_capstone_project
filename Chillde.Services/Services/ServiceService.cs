using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetAsync(Guid id)
        {
            try
            {
                var service = await _unitOfWork.ServiceRepository.GetAsync(id);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = service
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

        public async Task<ResponseModel> GetServiceAttachmentssAsync(Guid serviceId)
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

                Expression<Func<ServiceAttachment, bool>> filter = faq =>
                   faq.ServiceId == serviceId;

                var attachments = await _unitOfWork.ServiceAttachmentRepository.GetAllAsync(
                    filter: filter,
                    include: null
                    );

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = attachments
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

        public async Task<ResponseModel> AddPackageAsync(PackageAddModel packageAddModel, Guid serviceId)
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
                    Message = "Package successfully created.",
                    Data = package
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

        public async Task<ResponseModel> AddFAQAsync(FAQAddAndUpdateModel faqAddModel, Guid serviceId)
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

                var faq = new FAQ
                {
                    Question = faqAddModel.Question,
                    Answer = faqAddModel.Answer,
                    ServiceId = serviceId,
                };

                await _unitOfWork.FAQRepository.AddAsync(faq);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "FAQ successfully created.",
                    Data = faq
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

        public async Task<ResponseModel> GetAllFAQsAsync(Guid serviceId, FAQFilterModel faqFilterModel)
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

                Expression<Func<FAQ, bool>> filter = faq =>
                   faq.ServiceId == serviceId &&
                   faq.IsDeleted == faqFilterModel.IsDeleted;

                var faqs = await _unitOfWork.FAQRepository.GetAllAsync(
                    filter: filter,
                    include: null
                );

                var faqsModel = _mapper.Map<List<FAQModel>>(faqs.Data);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = faqsModel
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

        public async Task<ResponseModel> GetAllPackagesAsync(PackageFilterModel packageFilterModel, Guid serviceId)
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

                Expression<Func<Package, bool>> filter = package =>
                     package.ServiceId == serviceId &&
                     package.IsDeleted == packageFilterModel.IsDeleted &&
                     (string.IsNullOrEmpty(packageFilterModel.Search) ||
                     package.Name.Contains(packageFilterModel.Search));

                Func<IQueryable<Package>, IQueryable<Package>> include = packages =>
                         packages.Include(c => c.PackageFeatures).ThenInclude(_ => _.Feature);

                var packages = await _unitOfWork.PackageRepository.GetAllAsync(
                                filter: filter,
                                include: include,
                                pageIndex: packageFilterModel.PageIndex,
                pageSize: packageFilterModel.PageSize
                );

                var packageModels = _mapper.Map<List<PackageModel>>(packages.Data);

                var result = new Pagination<PackageModel>(packageModels, packageFilterModel.PageIndex,
                  packageFilterModel.PageSize, packages.TotalCount);


                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = result
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
