using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FAQModels;
using Chillde.Repositories.Models.FeedbackModels;
using Chillde.Services.Common;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Services.Helpers;
using Chillde.Services.Utils;

namespace Chillde.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IOpenAiService _openAiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IServiceAttachmentService _serviceAttachmentService;
        private readonly ITranslationService _translationService;
        private readonly IRedisHelper _redisHelper;

        public ServiceService(IOpenAiService openAiService, IUnitOfWork unitOfWork, IMapper mapper, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IServiceAttachmentService serviceAttachmentService, ITranslationService translationService, IRedisHelper redisHelper)
        {
            _openAiService = openAiService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _serviceAttachmentService = serviceAttachmentService;
            _translationService = translationService;
            _redisHelper = redisHelper;
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
                        attachment.AttachmentUrl!,
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
                Func<IQueryable<Service>, IQueryable<Service>> include = services =>
                     services.Include(_ => _.ServiceAttachments);

                var service = await _unitOfWork.ServiceRepository.GetAsync(id, include);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                var serviceModel = _mapper.Map<ServiceModel>(service);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = serviceModel
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
                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized."
                    };
                }

                var embeddingVector = await _openAiService.GetEmbeddingAsync(new List<string> { serviceAddModel.Description, serviceAddModel.Name });
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
                    IsOffer = serviceAddModel.IsOffer,
                    ItemId = serviceAddModel.ItemId,
                    Status = Repositories.Enums.ServiceStatus.Active,
                    EmbeddingVector = embeddingVector
                };

                await _unitOfWork.ServiceRepository.AddAsync(service);

                var newServiceAttachment = new List<ServiceAttachment>();
                var attachmentModel = serviceAddModel.ServiceAttachments;
                if (serviceAddModel.ServiceAttachments != null)
                {
                    for (int i = 0; i < attachmentModel!.Count; i++)
                    {
                        var attachmentAlt = attachmentModel[i].AttachmentAlt;
                        var attachmentUrl = attachmentModel[i].AttachmentUrls;

                        string? path = null;
                        if (attachmentUrl != null)
                        {
                            path = await _cloudinaryHelper.UploadImageAsync(
                                attachmentUrl,
                                "serviceAttachments",
                                Guid.NewGuid().ToString()
                            );
                        }

                        newServiceAttachment.Add(new ServiceAttachment
                        {
                            AttachmentAlt = attachmentAlt,
                            AttachmentUrl = path,
                            ServiceId = service.Id
                        });
                    }

                    await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(newServiceAttachment);
                    await _unitOfWork.SaveChangeAsync();

                    var serviceModel = _mapper.Map<ServiceModel>(service);
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status201Created,
                        Message = "Service successfully created.",
                        Data = serviceModel
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Service attachments are required."
                    };
                }
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

        public async Task<ResponseModel> UpdateAsync(ServiceUpdateModel serviceUpdateModel, Guid id)
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

                service.Status = serviceUpdateModel.Status;

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

                if (anyOrder.Result == false)
                {
                    _unitOfWork.ServiceRepository.HardRemove(service);
                }
                else
                {
                    _unitOfWork.ServiceRepository.SoftRemove(service);
                }
                
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

        public async Task<ResponseModel> AddPackageAsync(PackageAddModel packageAddModel, Guid serviceId, string sourceLanguageCode, string targetLanguageCode)
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
                await _unitOfWork.BeginTransactionAsync();
                var numberOfExistedPackage = _unitOfWork.PackageRepository.GetAllPackageFromService(serviceId).Result.Count();
                if (numberOfExistedPackage >= 3)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Number of packages cannot exceed 3."
                    };
                }
                var fieldsToTranslate = new Dictionary<string, string>
                {
                    { "Name", packageAddModel.Name },
                    { "Description", packageAddModel.Description }
                };
                var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                if (translationResponse.Code != StatusCodes.Status200OK)
                {
                    throw new Exception("Failed to translate fields.");
                }
                string translatedName = translationResponse.TranslatedFields["Name"];
                string translatedDescription = translationResponse.TranslatedFields["Description"];

                var package = new Package
                {
                    Name = sourceLanguageCode == "en" ? packageAddModel.Name : translatedName,
                    Description = sourceLanguageCode == "en" ? packageAddModel.Description : translatedName,
                    Price = packageAddModel.Price,
                    ServiceId = serviceId,
                };

                await _unitOfWork.PackageRepository.AddAsync(package);
                var translations = new List<Translation>();
                Guid? languageId = null;
                if (sourceLanguageCode != "en")
                {
                    languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(sourceLanguageCode);
                }
                else
                {
                    languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode);
                }
                if (!string.IsNullOrEmpty(packageAddModel.Name))
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Request",
                        EntityId = package.Id,
                        FieldName = "Name",
                        TranslationText = sourceLanguageCode != "en" ? packageAddModel.Name : translatedName,
                        LanguageId = languageId.Value
                    });
                }
                if (!string.IsNullOrEmpty(packageAddModel.Description))
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Request",
                        EntityId = package.Id,
                        FieldName = "Description",
                        TranslationText = sourceLanguageCode != "en" ? packageAddModel.Description : translatedDescription,
                        LanguageId = languageId.Value
                    });
                }
                await _unitOfWork.TranslationRepository.AddRangeAsync(translations);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                var packageModel = _mapper.Map<PackageModel>(package);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Package successfully created.",
                    Data = packageModel
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
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
                     package.Name!.Contains(packageFilterModel.Search));

                Func<IQueryable<Package>, IQueryable<Package>> include = packages =>
                         packages.Include(c => c.PackageFeatures)
                                 .ThenInclude(pf => pf.Feature);  // Include Feature Name

                var packages = await _unitOfWork.PackageRepository.GetAllAsync(
                                filter: filter,
                                include: include,
                                pageIndex: packageFilterModel.PageIndex,
                                pageSize: packageFilterModel.PageSize
                );

                var packageModels = packages.Data.Select(package => new PackageModel
                {
                    Id = package.Id,
                    Name = package.Name,
                    Description = package.Description,
                    Price = package.Price,
                    ServiceId = package.ServiceId,
                    IsDeleted = package.IsDeleted,
                    CreationDate = package.CreationDate,
                    Features = package.PackageFeatures
                        .GroupBy(pf => pf.Feature.Name) // Group by Feature Name
                        .Select(g => new FeatureModel
                        {
                            Name = g.Key,
                            PackageFeatures = g.ToList()
                        }).ToList()
                }).ToList();

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


        public async Task<ResponseModel> Search(ServiceFilterModel serviceFilterModel)
        {

            serviceFilterModel.Search = $"Find handmade services similar to: {serviceFilterModel.Search}";
            var inputEmbedding = await _openAiService.GetEmbeddingAsync(new List<string> { serviceFilterModel.Search });
            var services = await _unitOfWork.ServiceRepository.GetAllAsync();
            var threshold = 0.80;
            var keywordThreshold = 0.2;
            var tasks = services.Data
                                .Where(_ => _.EmbeddingVector != null && _.EmbeddingVector.Length > 0)
                                .Select(_ => Task.Run(() =>
                                {
                                    var serviceEmbedding = _.EmbeddingVector;
                                    var similarity = CosineSimilarity(inputEmbedding, serviceEmbedding);
                                    var keywordScore = (similarity < 0.5 &&
                                                        (_.Name!.Contains(serviceFilterModel.Search, StringComparison.OrdinalIgnoreCase) ||
                                                         _.Description!.Contains(serviceFilterModel.Search, StringComparison.OrdinalIgnoreCase)))
                                                        ? keywordThreshold : 0;

                                    return new ServiceModel
                                    {
                                        Id = _.Id,
                                        Name = _.Name!,
                                        Description = _.Description!,
                                        Similarity = similarity + keywordScore
                                    };
                                })).ToList();

            var results = (await Task.WhenAll(tasks))
                          .Where(_ => _.Similarity >= threshold)
                          .OrderByDescending(_ => _.Similarity)
                          .ToList();

            var result = new Pagination<ServiceModel>(
                results.Skip((serviceFilterModel.PageIndex - 1) * serviceFilterModel.PageSize)
                       .Take(serviceFilterModel.PageSize)
                       .ToList(),
                serviceFilterModel.PageIndex,
                serviceFilterModel.PageSize, results.Count);

            return new ResponseModel
            {
                Message = "Get all services successfully",
                Data = result
            };


        }

        public async Task<ResponseModel> GetAll(ServiceFilterModel serviceFilterModel)
        {
            var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                   filter: _ => _.IsDeleted == false,
                   include: _ => _.Include(_ => _.Packages).Include(_ => _.ServiceAttachments),
                   pageIndex: serviceFilterModel.PageIndex,
                   pageSize: serviceFilterModel.PageSize
                   );
            var serviceModels = services.Data.Select(_ => new ServiceModel
            {
                Id = _.Id,
                Name = _.Name!,
                Description = _.Description!,
                ServiceAttachments = _.ServiceAttachments.ToList()
            }).ToList();
            var result = new Pagination<ServiceModel>(
               serviceModels,
               serviceFilterModel.PageIndex,
               serviceFilterModel.PageSize,
               serviceModels.Count);

            return new ResponseModel
            {
                Message = "Get all services successfully",
                Data = result
            };
        }

        public async Task<ResponseModel> GetAllWithSuggestion(ServiceFilterModel serviceFilterModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;

            if (serviceFilterModel.IsSuggestion && currentUserId.HasValue)
            {
                var cacheKey = "suggested_services";
                var cacheDuration = TimeSpan.FromMinutes(15);

                var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    var recentLogs = await _unitOfWork.UserActivityLogRepository.GetAllAsync(
                        filter: log => log.UserId == currentUserId.Value,
                        order: q => q.OrderByDescending(log => log.Timestamp),
                        pageIndex: 1,
                        pageSize: 5
                    );

                    if (!recentLogs.Data.Any())
                    {
                        return new ResponseModel { Message = "No recent activity found for recommendations.", Data = null };
                    }

                    var averageEmbedding = ComputeAverageEmbedding(recentLogs.Data.Select(log => log.EmbeddingVector).ToList());

                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                        filter: _ => _.IsDeleted == false && _.IsOffer == false,
                        include: _ => _.Include(_ => _.Packages).Include(_ => _.ServiceAttachments),
                        pageIndex: serviceFilterModel.PageIndex,
                        pageSize: 1000
                     );

                    var threshold = 0.75;
                    var results = services.Data
                        .Where(s => s.EmbeddingVector != null && CosineSimilarity(averageEmbedding, s.EmbeddingVector) >= threshold)
                        .Select(s => new ServiceModel
                        {
                            Id = s.Id,
                            Name = s.Name!,
                            Description = s.Description!,
                            Similarity = CosineSimilarity(averageEmbedding, s.EmbeddingVector),
                            ServiceAttachments = s.ServiceAttachments.ToList(),
                            Rate = s.Rate,
                            FeedbackCount = s.FeedbackCount,
                            Price = s.Packages.OrderBy(p => p.Price).Select(p => p.Price).FirstOrDefault()
                        })
                        .OrderByDescending(s => s.Similarity)
                        .ToList();

                    var paginatedResult = new Pagination<ServiceModel>(
                        results,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        results.Count
                    );

                    return new ResponseModel
                    {
                        Message = "Get services based on user activity log successfully",
                        Data = paginatedResult
                    };
                }, cacheDuration);

                return responseModel;
            }
            else
            {
                var cacheKey = $"services_{CacheTools.GenerateCacheKey(serviceFilterModel)}";

                var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    Guid? filterId = null;
                    if (Guid.TryParse(serviceFilterModel.IdOrUserName, out var id))
                    {
                        filterId = id;
                    }
                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                        filter: s =>
                            s.IsDeleted == false &&
                            s.IsOffer == false &&
                            (string.IsNullOrEmpty(serviceFilterModel.IdOrUserName) || (filterId.HasValue && s.CreatedById == filterId.Value)
                            || s.CreatedBy.Username.Contains(serviceFilterModel.IdOrUserName)) &&
                            (!serviceFilterModel.ItemId.HasValue || s.ItemId == serviceFilterModel.ItemId) &&
                            (!serviceFilterModel.CategoryId.HasValue || s.Item.SubCategory.CategoryId == serviceFilterModel.CategoryId) &&
                            (!serviceFilterModel.SubCategoryId.HasValue || s.Item.SubCategoryId == serviceFilterModel.SubCategoryId) &&
                            (!serviceFilterModel.MinPrice.HasValue || s.Packages.Any(p => p.Price >= serviceFilterModel.MinPrice)) &&
                            (!serviceFilterModel.MaxPrice.HasValue || s.Packages.Any(p => p.Price <= serviceFilterModel.MaxPrice)) &&
                            (!serviceFilterModel.MinRate.HasValue || s.Rate >= serviceFilterModel.MinRate) &&
                            (!serviceFilterModel.MaxRate.HasValue || s.Rate <= serviceFilterModel.MaxRate) &&
                            (!serviceFilterModel.MinDate.HasValue || s.Packages.Any(p => p.DeliveryTime >= serviceFilterModel.MinDate.Value)) &&
                            (!serviceFilterModel.MaxDate.HasValue || s.Packages.Any(p => p.DeliveryTime <= serviceFilterModel.MaxDate.Value)),
                        order: s =>
                        {
                            switch (serviceFilterModel.Order.ToLower())
                            {
                                case "creating":
                                    return serviceFilterModel.OrderByDescending
                                        ? s.OrderByDescending(s => s.CreationDate)
                                        : s.OrderBy(s => s.CreationDate);
                                case "bestSelling":
                                    return serviceFilterModel.OrderByDescending
                                        ? s.OrderByDescending(s => s.Packages.Sum(p => p.Orders.Count))
                                        : s.OrderBy(s => s.Packages.Sum(p => p.Orders.Count));
                                default:
                                    return serviceFilterModel.OrderByDescending
                                       ? s.OrderByDescending(s => s.CreationDate)
                                       : s.OrderBy(s => s.CreationDate);
                            }
                        },
                        include: s => s.Include(p => p.Packages)
                                       .ThenInclude(p => p.Orders)
                                       .Include(a => a.ServiceAttachments)
                                       .Include(i => i.Item)
                                       .ThenInclude(i => i.SubCategory)
                                       .ThenInclude(su => su.Category)
                    );

                    var serviceModels = services.Data.Select(s => new ServiceModel
                    {
                        Id = s.Id,
                        Name = s.Name!,
                        Description = s.Description!,
                        ServiceAttachments = s.ServiceAttachments.ToList(),
                        Rate = s.Rate,
                        FeedbackCount = s.FeedbackCount,
                        Price = s.Packages.Any() ? s.Packages.Min(p => p.Price) : 0
                    }).ToList();

                    var paginatedResult = new Pagination<ServiceModel>(
                        serviceModels,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        services.TotalCount
                    );

                    return new ResponseModel
                    {
                        Message = "Get all services successfully",
                        Data = paginatedResult
                    };
                });

                return responseModel;
            }
        }


        private float[] ComputeAverageEmbedding(List<float[]> embeddings)
        {
            if (embeddings == null || embeddings.Count == 0) return new float[0];

            int dimension = embeddings[0].Length;
            float[] averageEmbedding = new float[dimension];

            foreach (var embedding in embeddings)
            {
                for (int i = 0; i < dimension; i++)
                {
                    averageEmbedding[i] += embedding[i];
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                averageEmbedding[i] /= embeddings.Count;
            }

            return averageEmbedding;
        }



        private static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length == 0 || vectorB.Length == 0)
                throw new ArgumentException("Embedding vectors cannot be empty.");

            var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
            var magnitudeA = Math.Sqrt(vectorA.Sum(a => a * a));
            var magnitudeB = Math.Sqrt(vectorB.Sum(b => b * b));

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

    }
}
