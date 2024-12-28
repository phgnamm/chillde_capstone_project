using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.FeedbackModels;
using Chillde.Repositories.Models.SubCategoryModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Xml.Linq;
using Chillde.Repositories.Enums;

namespace Chillde.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public ServiceService(IUnitOfWork unitOfWork, IMapper mapper, IClaimService claimService, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> AddFeedbackAsync(FeedbackAddModel feedbackAddModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }

            var hasCompletedOrder = await _unitOfWork.OrderRepository.HasCompletedOrder(currentUserId.Value, feedbackAddModel.ServiceId);
            if (!hasCompletedOrder)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "User has not completed an order in this service."
                };
            }
            /*
                        var hasFeedback = await _unitOfWork.FeedbackRepository.HasFeedback(currentUserId.Value, feedbackAddModel.ServiceId);
                        if (hasFeedback)
                        {
                            return new ResponseModel
                            {
                                Code = StatusCodes.Status400BadRequest,
                                Message = "User has already given feedback for this service."
                            };
                        }*/

            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                ServiceId = feedbackAddModel.ServiceId,
                CreatedById = currentUserId.Value,
                Rating = feedbackAddModel.Rating,
                Description = feedbackAddModel.Description,
            };

            await _unitOfWork.FeedbackRepository.AddAsync(feedback);
            if (feedbackAddModel.FeedbackAttachmentAddModels != null && feedbackAddModel.FeedbackAttachmentAddModels.Count > 0)
            {
                var feedbackAttachments = new List<FeedbackAttachment>();

                foreach (var attachment in feedbackAddModel.FeedbackAttachmentAddModels)
                {
                    var attachmentPath = await _cloudinaryHelper.UploadImageAsync(
                        attachment.AttachmentUrl,
                        "feedbacks",
                        feedback.Id.ToString()
                    );

                    feedbackAttachments.Add(new FeedbackAttachment
                    {
                        Id = Guid.NewGuid(),
                        FeedbackId = feedback.Id,
                        AttachmentUrl = attachmentPath,
                        AttachmentAlt = attachment.AttachmentAlt,
                    });
                }

                await _unitOfWork.FeedbackAttachmentRepository.AddRangeAsync(feedbackAttachments);
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Feedback created successfully.",
            };
        }

        public async Task<ResponseModel> GetAllFeedbacksByServiceAndUserAsync(Guid serviceId, FeedbackFilterModel feedbackFilterModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAllAsync(
                filter: _ => _.IsDeleted == feedbackFilterModel.IsDeleted && _.ServiceId == serviceId && _.CreatedById == currentUserId.Value,
                include: feedbacks => feedbacks.Include(_ => _.FeedbackAttachments)
                                               .Include(_ => _.CreatedBy)
                                               .Include(_ => _.Service),
                pageIndex: feedbackFilterModel.PageIndex,
                pageSize: feedbackFilterModel.PageSize
            );
            var feedbackModels = feedbacks.Data.Select(_ => new FeedbackModel
            {
                Id = _.Id,
                CreatedById = _.CreatedById,
                ServiceId = _.ServiceId,
                AuthorName = _.CreatedBy.FirstName + " " + _.CreatedBy.LastName,
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackImageModels = _.FeedbackAttachments.Select(_ => new FeedbackImageModel
                {
                    ImageUrl = _.AttachmentUrl ?? ""
                }).ToList()
            }).ToList();
            var result = new Pagination<FeedbackModel>(feedbackModels, feedbackFilterModel.PageIndex,
                feedbackFilterModel.PageSize, feedbacks.TotalCount);

            return new ResponseModel
            {
                Message = "Get all feedbacks successfully",
                Data = result
            };
        }

        public async Task<ResponseModel> GetAllFeedbacksByServiceAsync(Guid serviceId, FeedbackFilterModel feedbackFilterModel)
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAllAsync(
                filter: _ => _.IsDeleted == feedbackFilterModel.IsDeleted && _.ServiceId == serviceId,
                include: feedbacks => feedbacks.Include(_ => _.FeedbackAttachments)
                                               .Include(_ => _.CreatedBy)
                                               .Include(_ => _.Service),
                pageIndex: feedbackFilterModel.PageIndex,
                pageSize: feedbackFilterModel.PageSize
            );
            var feedbackModels = feedbacks.Data.Select(_ => new FeedbackModel
            {
                Id = _.Id,
                CreatedById = _.CreatedById,
                ServiceId = _.ServiceId,
                AuthorName = _.CreatedBy.FirstName + " " + _.CreatedBy.LastName,
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackImageModels = _.FeedbackAttachments.Select(_ => new FeedbackImageModel
                {
                    ImageUrl = _.AttachmentUrl ?? ""
                }).ToList()
            }).ToList();
            var result = new Pagination<FeedbackModel>(feedbackModels, feedbackFilterModel.PageIndex,
                feedbackFilterModel.PageSize, feedbacks.TotalCount);

            return new ResponseModel
            {
                Message = "Get all feedbacks successfully",
                Data = result
            };
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

        public async Task<ResponseModel> AddAsync(ServiceAddModel serviceAddModel)
        {
            try
            {
                var item = await _unitOfWork.ItemRepository.GetAsync(serviceAddModel.ItemId);
                if (item == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Item not found."
                    };
                }

                var service = new Service
                {
                    Name = serviceAddModel.Name,
                    Description = serviceAddModel.Description,
                    IsOffter = serviceAddModel.IsOffter,
                    ItemId = serviceAddModel.ItemId,
                    Status = Repositories.Enums.ServiceStatus.Active
                };

                await _unitOfWork.ServiceRepository.AddAsync(service);

                var serviceAttachmentList = new List<ServiceAttachment>();
                foreach (var serviceAttachment in serviceAddModel.ServiceAttachments) 
                {
                    string? imageUrl = null;
                    if (serviceAttachment.AttachmentUrl != null)
                    {
                        imageUrl = await _cloudinaryHelper.UploadImageAsync(
                            serviceAttachment.AttachmentUrl,
                            serviceAttachment.AttachmentAlt,
                            Guid.NewGuid().ToString()
                        );
                    }
                    serviceAttachmentList.Add(new ServiceAttachment
                    {
                        AttachmentAlt = serviceAttachment.AttachmentAlt,
                        AttachmentUrl = serviceAttachment?.AttachmentUrl?.ToString(),
                        ServiceId = service.Id
                    });
                }

                await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(serviceAttachmentList);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Service successfully created.",
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

        public async Task<ResponseModel> UpdateAsync(ServiceStatus serviceStatus, Guid id)
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

                service.Status = serviceStatus;

                _unitOfWork.ServiceRepository.Update(service);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Service successfully updated.",
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
                var service = await _unitOfWork.ServiceRepository.GetAsync(id);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrder(id);

                if (anyOrder == null)
                {
                    _unitOfWork.ServiceRepository.HardRemove(service);
                }

                _unitOfWork.ServiceRepository.SoftRemove(service);
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
