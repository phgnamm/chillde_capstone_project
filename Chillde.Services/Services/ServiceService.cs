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
using Chillde.Services.Utils;
using Nest;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Models.ServiceAttachmentModels;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.ServiceAttachmentModels;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Chillde.Repositories.Common;
using Chillde.Repositories.Models.UserActivityLogModels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Chillde.Repositories.Models.ServiceWishlistModels;

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
        private readonly KeywordGenerator _keywordGenerator;
        private readonly IElasticClient _client;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IBadWordFilterService _badWordFilterService;


        public ServiceService(IElasticClient client,
            IOpenAiService openAiService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimService claimService,
            ICloudinaryHelper cloudinaryHelper,
            IServiceAttachmentService serviceAttachmentService,
            ITranslationService translationService,
            IRedisHelper redisHelper,
            IBadWordFilterService badWordFilterService, ISystemConfigService systemConfigService)
        {
            _client = client;
            _openAiService = openAiService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _serviceAttachmentService = serviceAttachmentService;
            _translationService = translationService;
            _redisHelper = redisHelper;
            _systemConfigService = systemConfigService;
            _keywordGenerator = new KeywordGenerator();
            _badWordFilterService = badWordFilterService;
        }

        public async Task<ResponseModel> AddFeedbackAsync(FeedbackAddModel feedbackAddModel, string sourceLanguageCode)
        {
            string[] fieldsToCheck = { feedbackAddModel.Description! };

            foreach (var field in fieldsToCheck)
            {
                ResponseModel response = sourceLanguageCode == "vi"
                    ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                    : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                if (response.Code == StatusCodes.Status422UnprocessableEntity)
                    return response;
            }

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

            var hasFeedback = await _unitOfWork.FeedbackRepository.HasFeedback(currentUserId.Value, feedbackAddModel.ServiceId);
            if (hasFeedback)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "User has already given feedback for this service."
                };
            }
            var existService = await _unitOfWork.ServiceRepository.GetAsync(feedbackAddModel.ServiceId);
            if (existService == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Service not found"
                };
            }
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                ServiceId = feedbackAddModel.ServiceId,
                ArtisanId = existService.CreatedById ?? Guid.Empty,
                CreatedById = currentUserId.Value,
                Rating = feedbackAddModel.Rating,
                Description = feedbackAddModel.Description,
            };

            await _unitOfWork.FeedbackRepository.AddAsync(feedback);
            if (feedbackAddModel.FeedbackAttachmentAddModels.Count > 0)
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
            var service = await _unitOfWork.ServiceRepository.GetAsync(feedbackAddModel.ServiceId);
            if (service != null)
            {
                service.Rate = CaculateRating((double)service.Rate!, (int)service.FeedbackCount!, (double)feedback.Rating);
                service.FeedbackCount++;
                _unitOfWork.ServiceRepository.Update(service);
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
                CreatedBy = new AccountLiteModel
                {
                    FirstName = _.CreatedBy.FirstName,
                    LastName = _.CreatedBy.LastName,
                    Username = _.CreatedBy.Username,
                    Email = _.CreatedBy.Email,
                    Image = _.CreatedBy.Image
                },
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackAttachmentModels = _.FeedbackAttachments.Select(_ => new FeedbackAttachmentModel
                {
                   AttachmentAlt = _.AttachmentAlt,
                   AttachmentUrl = _.AttachmentUrl,
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
                CreatedBy = new AccountLiteModel
                {
                    FirstName = _.CreatedBy.FirstName,
                    LastName = _.CreatedBy.LastName,
                    Username = _.CreatedBy.Username,
                    Email = _.CreatedBy.Email,
                    Image = _.CreatedBy.Image
                },
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackAttachmentModels = _.FeedbackAttachments.Select(_ => new FeedbackAttachmentModel
                {
                    AttachmentAlt = _.AttachmentAlt,
                    AttachmentUrl = _.AttachmentUrl,
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
                var cacheKey = $"services_{id}";
                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
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
                });
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

        public async Task<ResponseModel> AddAsync(ServiceAddModel serviceAddModel, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                var validationResult = await ValidateServiceAsync(serviceAddModel, sourceLanguageCode);
                if (validationResult != null)
                    return validationResult;

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

                var category = await _unitOfWork.CategoryRepository.GetAsync(serviceAddModel.CategoryId);
                if (category == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Category not found."
                    };
                }

                var numberOfExistedService = _unitOfWork.ServiceRepository.GetAllAsync(
                    _ => _.CreatedById == currentUserId && _.IsDeleted == false).Result.TotalCount;
                var maximumService = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumSerivceOfOneArtisan).Result;
                if (numberOfExistedService > int.Parse(maximumService!))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = $"Number of services cannot exceed {maximumService}."
                    };
                }

                var service = new Service
                {
                    Name = serviceAddModel.Name,
                    Description = serviceAddModel.Description,
                    Status = ServiceStatus.Inactive,
                    MinWeight = serviceAddModel.MinWeight,
                    MaxWeight = serviceAddModel.MaxWeight,
                    CategoryId = serviceAddModel.CategoryId,
                    EmbeddingVector = embeddingVector,
                    FeedbackCount = 0,
                    Rate = 0
                };

                await _unitOfWork.ServiceRepository.AddAsync(service);
                await EnsureElasticsearchIndexExistsAsync("test_keywords1");

                var keywords = _keywordGenerator.GenerateKeywords(service.Name.ToLower());
                service.Keywords = keywords;
                var elasticResult = await IndexKeywordsAsync("test_keywords1", keywords);
                if (!elasticResult)
                    return new ResponseModel { Message = "Failed to insert keywords into Elasticsearch.", Code = StatusCodes.Status500InternalServerError };
                //await _client.IndexAsync(new { id = service.Id, embeddingVector = service.EmbeddingVector }, i => i.Index("test_embedding"));
                var serviceResult = await IndexServiceAsync("test_service1", service);
                if (!serviceResult)
                {
                    return new ResponseModel
                    {
                        Message = "Failed to insert service into Elasticsearch.",
                        Code = StatusCodes.Status500InternalServerError
                    };
                }
                var newServiceAttachment = new List<ServiceAttachment>();
                var attachmentModel = serviceAddModel.ServiceAttachments;
                if (serviceAddModel.ServiceAttachments != null)
                {
                    for (int i = 0; i < attachmentModel!.Count; i++)
                    {
                        var attachmentAlt = attachmentModel[i].AttachmentAlt;
                        var attachmentUrl = attachmentModel[i].AttachmentUrl;

                        string? path = null;
                        if (attachmentUrl != null)
                        {
                            path = await _cloudinaryHelper.UploadImageAsync(
                                attachmentUrl,
                                attachmentAlt,
                                Guid.NewGuid().ToString(),
                                folderName: FolderAttachment.SERVICE
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
                //else
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status400BadRequest,
                //        Message = "Service attachments are required."
                //    };
                //}
                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Service successfully created.",
                    //Data = serviceModel
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

        private async Task<ResponseModel> ValidateServiceAsync(ServiceAddModel model, string sourceLang)
        {
            var fields = new[] { model.Name, model.Description };
            foreach (var field in fields)
            {
                var response = sourceLang == "vi"
                    ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                    : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                if (response.Code == StatusCodes.Status422UnprocessableEntity)
                    return response;
            }
            return null;
        }

        private async Task EnsureElasticsearchIndexExistsAsync(string indexName)
        {
            var exists = await _client.Indices.ExistsAsync(indexName);
            if (exists.Exists)
                return;

            await _client.Indices.CreateAsync(indexName, c => c
                .Settings(s => s.Analysis(a => a
                    .Tokenizers(t => t.EdgeNGram("edge_ngram_tokenizer", e => e
                        .MinGram(1).MaxGram(20)
                        .TokenChars(TokenChar.Letter, TokenChar.Digit)))
                    .Analyzers(an => an.Custom("edge_ngram_analyzer", ca => ca
                        .Tokenizer("edge_ngram_tokenizer")
                        .Filters("lowercase")))))
                .Map<object>(m => m.Properties(p => p
                    .Text(t => t.Name("keyword")
                        .Analyzer("edge_ngram_analyzer")
                        .SearchAnalyzer("standard"))
                    .Completion(c => c.Name("suggest")))));
        }

        private async Task<bool> IndexKeywordsAsync(string indexName, IEnumerable<string> keywords)
        {
            var bulkOps = keywords.Select(keyword => new
            {
                id = Guid.NewGuid(),
                keyword,
                suggest = new { input = new[] { keyword }, weight = 1 }
            });

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(indexName)
                .IndexMany(bulkOps));

            return !bulkResponse.Errors;
        }

        private async Task<bool> IndexServiceAsync(string indexName, Service service)
        {
            var serviceDocument = new
            {
                id = service.Id,
                name = service.Name,
                description = service.Description,
                keywords = service.Keywords,
                embeddingVector = service.EmbeddingVector,
                categoryId = service.CategoryId,
                minWeight = service.MinWeight,
                maxWeight = service.MaxWeight,
                status = service.Status
            };

            var response = await _client.IndexAsync(serviceDocument, i => i.Index(indexName));
            return response.IsValid;
        }

        public async Task<ResponseModel> UpdateAsync(ServiceUpdateModel serviceUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { serviceUpdateModel.Name, serviceUpdateModel.Description };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var service = await _unitOfWork.ServiceRepository.GetAsync(id);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized."
                    };
                }

                ServiceModel serviceModel = new ServiceModel();
                var anyOrderOfService = _unitOfWork.OrderRepository.HasAnyOrderByService(id);

                if (!anyOrderOfService.Result)
                {
                    _mapper.Map(serviceUpdateModel, service);
                    _unitOfWork.ServiceRepository.Update(service);
                    serviceModel = _mapper.Map<ServiceModel>(service);
                }
                else
                {
                    _unitOfWork.ServiceRepository.SoftRemove(service);
                    Service newService = _mapper.Map<Service>(serviceUpdateModel);
                    newService.CreatedById = service.CreatedById;
                    newService.CategoryId = service.CategoryId;
                    await _unitOfWork.ServiceRepository.AddAsync(newService);

                    var serviceAttachments = await _unitOfWork.ServiceAttachmentRepository.GetAllAsync(
                        filter: _ => _.ServiceId == id && _.IsDeleted == false
                    );

                    var serviceAttachmentWithNewService = new List<ServiceAttachment>();
                    foreach (var serviceAttachment in serviceAttachments.Data)
                    {
                        serviceAttachment.Id = new Guid();
                        serviceAttachment.ServiceId = newService.Id;
                        serviceAttachmentWithNewService.Add(serviceAttachment);
                    }
                    await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(serviceAttachmentWithNewService);

                    serviceModel = _mapper.Map<ServiceModel>(newService);
                    serviceModel.ServiceAttachments = serviceAttachmentWithNewService;
                }

                if (serviceUpdateModel.ServiceAttachments != null)
                {
                    var newServiceAttachments = new List<ServiceAttachment>();

                    for (int i = 0; i < serviceUpdateModel.ServiceAttachments.Count; i++)
                    {
                        var attachmentAlt = serviceUpdateModel.ServiceAttachments[i].AttachmentAlt;
                        var attachmentUrl = serviceUpdateModel.ServiceAttachments[i].AttachmentUrl;

                        Guid Id = Guid.NewGuid();

                        string? path = null;
                        if (attachmentUrl != null)
                        {
                            path = await _cloudinaryHelper.UploadImageAsync(
                                attachmentUrl,
                                attachmentAlt,
                                Id.ToString(),
                                folderName: FolderAttachment.SERVICE
                            );
                        }

                        newServiceAttachments.Add(new ServiceAttachment
                        {
                            Id = Id,
                            AttachmentAlt = attachmentAlt,
                            AttachmentUrl = path,
                            ServiceId = service.Id
                        });
                    }

                    await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(newServiceAttachments);
                }

                if (serviceUpdateModel.ServiceAttachmentIdsDeleting != null)
                {
                    var serviceAttachments = await _unitOfWork.ServiceAttachmentRepository.GetAllAsync(
                    filter: _ => serviceUpdateModel.ServiceAttachmentIdsDeleting.Contains(_.Id)
                    );

                    if (serviceAttachments == null || !serviceAttachments.Data.Any())
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "Attachments not found."
                        };
                    }

                    var publicIds = serviceAttachments.Data.Select(a => a.Id).ToList();

                    await _cloudinaryHelper.RemoveImagesAsync(serviceUpdateModel.ServiceAttachmentIdsDeleting.Select(id => id.ToString()).ToList());

                    _unitOfWork.ServiceAttachmentRepository.HardRemoveRange(serviceAttachments.Data);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"services_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("services_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Service successfully updated.",
                        Data = serviceModel
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
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

        public async Task<ResponseModel> ActiveAsync(Guid id)
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

                service.Status = ServiceStatus.Active;
                _unitOfWork.ServiceRepository.Update(service);

                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Active successfully."
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

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByService(id);

                if (anyOrder.Result == false)
                {
                    _unitOfWork.ServiceRepository.HardRemove(service);
                }
                else
                {
                    _unitOfWork.ServiceRepository.SoftRemove(service);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"services_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("services_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully delete."
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
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

        public async Task<ResponseModel> AddListServiceAttachmentAsync(List<AttachmentAddModel> attachmentModel, Guid serviceId)
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

                var newServiceAttachment = new List<ServiceAttachment>();

                for (int i = 0; i < attachmentModel.Count; i++)
                {
                    var attachmentAlt = attachmentModel[i].AttachmentAlt;
                    var attachmentUrl = attachmentModel[i].AttachmentUrl;
                    Guid Id = Guid.NewGuid();
                    string? path = null;
                    if (attachmentUrl != null)
                    {
                        path = await _cloudinaryHelper.UploadImageAsync(
                            attachmentUrl,
                            attachmentAlt,
                            Id.ToString(),
                            folderName: FolderAttachment.SERVICE
                        );
                    }

                    newServiceAttachment.Add(new ServiceAttachment
                    {
                        Id = Id,
                        AttachmentAlt = attachmentAlt,
                        AttachmentUrl = path,
                        ServiceId = service.Id
                    });
                }

                await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(newServiceAttachment);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Success"
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
                string[] fieldsToCheck = { packageAddModel.Description };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var service = await _unitOfWork.ServiceRepository.GetAsync(serviceId);
                if (service == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Service not found."
                    };
                }

                var packageWithSameName = _unitOfWork.PackageRepository.GetPackageByNameAsync(packageAddModel.Name, serviceId);
                if (packageWithSameName)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Service already has this package's name."
                    };
                }

                var maxPriceOfPackage = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaxPriceOfPackage).Result;
                if (packageAddModel.Price <= 0 && packageAddModel.Price > int.Parse(maxPriceOfPackage!))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Price must be greater than 0 and less than 10,000,000."
                    };
                }

                //await _unitOfWork.BeginTransactionAsync();

                var numberOfExistedPackage = _unitOfWork.PackageRepository.GetAllPackageFromService(serviceId).Result.Count();
                var maximumPackage = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumPackageOfOneService).Result;
                if (numberOfExistedPackage > int.Parse(maximumPackage!))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = $"Number of packages cannot exceed {maximumPackage}."
                    };
                }
                //var fieldsToTranslate = new Dictionary<string, string>
                //{
                //    { "Name", packageAddModel.Name },
                //    { "Description", packageAddModel.Description }
                //};
                //var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                ////if (translationResponse.Code != StatusCodes.Status200OK)
                ////{
                ////    throw new Exception("Failed to translate fields.");
                ////}
                //string translatedName = translationResponse.TranslatedFields["Name"];
                //string translatedDescription = translationResponse.TranslatedFields["Description"];

                //var package = new Package
                //{
                //    //Name = sourceLanguageCode == "en" ? packageAddModel.Name : translatedName,
                //    //Description = sourceLanguageCode == "en" ? packageAddModel.Description : translatedName,
                //    Name = packageAddModel.Name,
                //    Description = packageAddModel.Description,
                //    Price = packageAddModel.Price,
                //    ServiceId = serviceId,
                //    DeliveryTime = packageAddModel.DeliveryTime,
                //    SketchRevision = packageAddModel.SketchRevision,
                //    ResponseTime = packageAddModel.ResponseTime
                //};

                var package = _mapper.Map<Package>(packageAddModel);
                //package.ResponseTime = (float)packageAddModel.ResponseTime.TotalMinutes;
                package.ServiceId = serviceId;

                await _unitOfWork.PackageRepository.AddAsync(package);
                //var translations = new List<Translation>();
                //Guid? languageId = null;
                //if (sourceLanguageCode != "en")
                //{
                //    languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(sourceLanguageCode);
                //}
                //else
                //{
                //    languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode);
                //}
                //if (!string.IsNullOrEmpty(packageAddModel.Name))
                //{
                //    translations.Add(new Translation
                //    {
                //        Id = Guid.NewGuid(),
                //        EntityType = "Request",
                //        EntityId = package.Id,
                //        FieldName = "Name",
                //        TranslationText = sourceLanguageCode != "en" ? packageAddModel.Name : translatedName,
                //        LanguageId = languageId.Value
                //    });
                //}
                //if (!string.IsNullOrEmpty(packageAddModel.Description))
                //{
                //    translations.Add(new Translation
                //    {
                //        Id = Guid.NewGuid(),
                //        EntityType = "Request",
                //        EntityId = package.Id,
                //        FieldName = "Description",
                //        TranslationText = sourceLanguageCode != "en" ? packageAddModel.Description : translatedDescription,
                //        LanguageId = languageId.Value
                //    });
                //}
                //await _unitOfWork.TranslationRepository.AddRangeAsync(translations);
                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

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

        public async Task<ResponseModel> AddFAQAsync(FAQAddAndUpdateModel faqAddModel, Guid serviceId, string sourceLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { faqAddModel.Question, faqAddModel.Answer };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

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
                    Data = _mapper.Map<FAQModel>(faq)
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

                var cacheKey = $"faqs_{CacheTools.GenerateCacheKey(faqFilterModel)}";

                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    Expression<Func<FAQ, bool>> filter = faq =>
                   faq.ServiceId == serviceId &&
                   faq.IsDeleted == faqFilterModel.IsDeleted;

                    var faqs = await _unitOfWork.FAQRepository.GetAllAsync(
                        filter: filter,
                        include: null
                    );

                    var faqsModel = _mapper.Map<List<FAQModel>>(faqs.Data);

                    var result = new Pagination<FAQModel>(faqsModel, faqFilterModel.PageIndex,
                      faqFilterModel.PageSize, faqs.TotalCount);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully.",
                        Data = result
                    };
                });
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

        public async Task<ResponseModel> GetAllPackagesByServiceAsync(PackageFilterModel packageFilterModel, Guid serviceId)
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

                var cacheKey = $"packages_{CacheTools.GenerateCacheKey(packageFilterModel)}";

                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    Expression<Func<Package, bool>> filter = package =>
                     package.ServiceId == serviceId &&
                     package.IsDeleted == packageFilterModel.IsDeleted &&
                     (string.IsNullOrEmpty(packageFilterModel.Search)
                     );

                    Func<IQueryable<Package>, IQueryable<Package>> include = packages =>
                             packages.Include(c => c.PackageFeatures)
                                     .ThenInclude(pf => pf.Feature);

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
                        DeliveryTime = package.DeliveryTime,
                        SketchRevision = package.SketchRevision,
                        ResponseTime = TimeSpan.FromMinutes(package.ResponseTime),
                        ServiceId = package.ServiceId,
                        IsDeleted = package.IsDeleted,
                        MinQuantity = package.MinQuantity,
                        MaxQuantity = package.MaxQuantity,
                        CreationDate = package.CreationDate,
                        Features = package.PackageFeatures
                            .GroupBy(pf => pf.Feature.Name)
                            .Select(g => new FeatureModel
                            {
                                Id = g.First().FeatureId,
                                Name = g.Key,
                                Question = g.First().Feature.Question,
                                QuestionType = g.First().Feature.QuestionType,
                                IsInformationRequired = g.First().Feature.IsInformationRequired,
                                IsQuantity = g.First().Feature.IsQuantity,
                                PackageFeatures = g.ToList()
                            }).ToList()
                    }).OrderBy(_ => _.Name).ToList();

                    var result = new Pagination<PackageModel>(packageModels, packageFilterModel.PageIndex,
                      packageFilterModel.PageSize, packages.TotalCount);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully.",
                        Data = result
                    };
                });
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

        public async Task<ResponseModel> GetAllFeaturesByServiceAsync(Guid serviceId)
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

                var cacheKey = $"features_{CacheTools.GenerateCacheKey(serviceId)}";

                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    var feature = await _unitOfWork.PackageRepository.GetAllFeatureByService(serviceId);
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully.",
                        Data = feature
                    };
                });
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
            if (string.IsNullOrWhiteSpace(serviceFilterModel.Search))
            {
                return new ResponseModel { Message = "Search term is required.", Data = null };
            }

            int pageIndex = serviceFilterModel.PageIndex;
            int pageSize = serviceFilterModel.PageSize;

            var result = new Pagination<ServiceModel>(null!, pageIndex, pageSize, 0);

            var currentUserId = _claimService.GetCurrentUserId;

            if (currentUserId.HasValue)
            {
                await SaveSearchHistoryAsync(serviceFilterModel.Search, currentUserId.Value);
            }

            var exactMatchResults = await SearchExactMatch(serviceFilterModel, pageIndex, pageSize);
            if (exactMatchResults != null)
            {
                return new ResponseModel
                {
                    Message = "Search results found by exact keyword match.",
                    Data = exactMatchResults
                };
            }
            var embeddingResults = await SearchByEmbedding(serviceFilterModel, pageIndex, pageSize);
            if (embeddingResults != null)
            {
                return new ResponseModel
                {
                    Message = "Search results found.",
                    Data = embeddingResults
                };
            }
            var fuzzyMatchResults = await SearchFuzzyMatch(serviceFilterModel, pageIndex, pageSize);
            if (fuzzyMatchResults != null)
            {
                return new ResponseModel
                {
                    Message = "Search results found by fuzzy match.",
                    Data = fuzzyMatchResults
                };
            }
            return new ResponseModel
            {
                Message = "No results found for the given query.",
                Data = null
            };
        }

        private async Task<Pagination<ServiceModel>?> SearchFuzzyMatch(ServiceFilterModel serviceFilterModel, int pageIndex, int pageSize)
        {
            var fuzzySearchResponse = await _client.SearchAsync<Service>(s => s
                .Index("test_service")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            bs => bs.MultiMatch(m => m
                                .Fields(f => f
                                    .Field(p => p.Name, 2.0)
                                    .Field(p => p.Description)
                                    .Field(p => p.Keywords))
                                .Query(serviceFilterModel.Search)
                                .Fuzziness(Fuzziness.EditDistance(2))
                            )
                        )
                    )
                )
                .From((pageIndex - 1) * pageSize)
                .Size(pageSize)
            );

            if (!fuzzySearchResponse.Documents.Any()) return null;

            var serviceIds = fuzzySearchResponse.Documents.Select(doc => doc.Id).ToList();

            var dbServices = await _unitOfWork.ServiceRepository.GetAllAsync(filter: _ => serviceIds.Contains(_.Id), include: _ => _.Include(_ => _.CreatedBy).Include(_ => _.ServiceAttachments));

            var serviceModels = dbServices.Data.Select(dbService =>
            {
                var esDocument = fuzzySearchResponse.Documents.FirstOrDefault(_ => _.Id == dbService.Id);

                return new ServiceModel
                {
                    Id = dbService.Id,
                    Name = esDocument?.Name ?? dbService.Name ?? "Unknown",
                    Description = esDocument?.Description ?? dbService.Description ?? "No description available",
                    FeedbackCount = dbService.FeedbackCount,
                    Rate = dbService.Rate,
                    MinWeight = dbService.MinWeight,
                    MaxWeight = dbService.MaxWeight,
                    ServiceAttachments = dbService.ServiceAttachments?.ToList(),
                    Artisan = dbService.CreatedBy == null ? null : new AccountLiteModel
                    {
                        FirstName = dbService.CreatedBy.FirstName ?? "Unknown",
                        LastName = dbService.CreatedBy.LastName ?? "Unknown",
                        Username = dbService.CreatedBy.Username ?? "Unknown",
                        Email = dbService.CreatedBy.Email ?? "Unknown",
                        Image = dbService.CreatedBy.Image ?? "Unknown"
                    }
                };
            }).ToList();

            return new Pagination<ServiceModel>(serviceModels, pageIndex, pageSize, (int)fuzzySearchResponse.Total);
        }

        private async Task<Pagination<ServiceModel>?> SearchExactMatch(ServiceFilterModel serviceFilterModel, int pageIndex, int pageSize)
        {
            var exactMatchResponse = await _client.SearchAsync<Service>(s => s
                .Index("test_service")
                .Query(q => q
                    .Terms(t => t
                        .Field(p => p.Keywords.Suffix("keyword"))
                        .Terms(serviceFilterModel.Search.ToLower().Trim())
                    )
                )
                .From((pageIndex - 1) * pageSize)
                .Size(pageSize)
            );

            if (!exactMatchResponse.Documents.Any()) return null;

            var serviceIds = exactMatchResponse.Documents.Select(doc => doc.Id).ToList();

            var dbServices = await _unitOfWork.ServiceRepository.GetAllAsync(filter: _ => serviceIds.Contains(_.Id), include: _ => _.Include(_ => _.CreatedBy).Include(_ => _.ServiceAttachments));

            var serviceModels = dbServices.Data.Select(dbService =>
            {
                var esDocument = exactMatchResponse.Documents.FirstOrDefault(_ => _.Id == dbService.Id);

                return new ServiceModel
                {
                    Id = dbService.Id,
                    Name = esDocument?.Name ?? dbService.Name ?? "Unknown",
                    Description = esDocument?.Description ?? dbService.Description ?? "No description available",
                    FeedbackCount = dbService.FeedbackCount,
                    Rate = dbService.Rate,
                    MinWeight = dbService.MinWeight,
                    MaxWeight = dbService.MaxWeight,
                    ServiceAttachments = dbService.ServiceAttachments?.ToList(),
                    Artisan = dbService.CreatedBy == null ? null : new AccountLiteModel
                    {
                        FirstName = dbService.CreatedBy.FirstName ?? "Unknown",
                        LastName = dbService.CreatedBy.LastName ?? "Unknown",
                        Username = dbService.CreatedBy.Username ?? "Unknown",
                        Email = dbService.CreatedBy.Email ?? "Unknown",
                        Image = dbService.CreatedBy.Image ?? "Unknown"
                    }
                };
            }).ToList();

            return new Pagination<ServiceModel>(serviceModels, pageIndex, pageSize, (int)exactMatchResponse.Total);
        }

        private async Task<Pagination<ServiceModel>?> SearchByEmbedding(ServiceFilterModel serviceFilterModel, int pageIndex, int pageSize)
        {
            float[] inputEmbedding = await _openAiService.GetEmbeddingAsync(new List<string> { serviceFilterModel.Search });

            if (inputEmbedding == null || inputEmbedding.Length == 0)
            {
                return null;
            }

            var magnitude = Math.Sqrt(inputEmbedding.Sum(x => x * x));
            if (magnitude == 0)
            {
                return null;
            }

            var normalizedEmbedding = inputEmbedding.Select(x => (float)(x / magnitude)).ToArray();

            var embeddingSearchResponse = await _client.SearchAsync<Service>(s => s
                .Index("test_service")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            bs => bs.ScriptScore(ss => ss
                                .Query(qq => qq.MatchAll())
                                .Script(script => script
                                    .Source("doc['embeddingVector'] != null ? cosineSimilarity(params.query_vector, doc['embeddingVector']) + 1.0 : 0")
                                    .Params(p => p.Add("query_vector", normalizedEmbedding))
                                )
                            ),
                            bs => bs.MultiMatch(m => m
                                .Fields(f => f
                                    .Field(p => p.Name, 2.0)
                                    .Field(p => p.Description)
                                    .Field(p => p.Keywords))
                                .Query(serviceFilterModel.Search)
                                .Fuzziness(Fuzziness.EditDistance(2))
                            )
                        )
                        .MinimumShouldMatch(1)
                    )
                )
                .Sort(s => s.Descending(SortSpecialField.Score))
                .From((pageIndex - 1) * pageSize)
                .Size(pageSize)
            );

            if (!embeddingSearchResponse.Documents.Any())
            {
                return null;
            }
            var serviceIds = embeddingSearchResponse.Documents.Select(doc => doc.Id).ToList();

            var dbServices = await _unitOfWork.ServiceRepository.GetAllAsync(filter: _ => serviceIds.Contains(_.Id), include: _ => _.Include(_ => _.CreatedBy).Include(_ => _.ServiceAttachments));

            var serviceModels = dbServices.Data.Select(dbService =>
            {
                var esDocument = embeddingSearchResponse.Documents.FirstOrDefault(doc => doc.Id == dbService.Id);

                return new ServiceModel
                {
                    Id = dbService.Id,
                    Name = esDocument?.Name ?? dbService.Name ?? "Unknown",
                    Description = esDocument?.Description ?? dbService.Description ?? "No description available",
                    FeedbackCount = dbService.FeedbackCount,
                    Rate = dbService.Rate,
                    MinWeight = dbService.MinWeight,
                    MaxWeight = dbService.MaxWeight,
                    ServiceAttachments = dbService.ServiceAttachments?.ToList(),
                    Artisan = dbService.CreatedBy == null ? null : new AccountLiteModel
                    {
                        FirstName = dbService.CreatedBy.FirstName ?? "Unknown",
                        LastName = dbService.CreatedBy.LastName ?? "Unknown",
                        Username = dbService.CreatedBy.Username ?? "Unknown",
                        Email = dbService.CreatedBy.Email ?? "Unknown",
                        Image = dbService.CreatedBy.Image ?? "Unknown"
                    }
                };
            }).ToList();

            return new Pagination<ServiceModel>(serviceModels, pageIndex, pageSize, (int)embeddingSearchResponse.Total);
        }

        public async Task<ResponseModel> GetAll(ServiceFilterModel serviceFilterModel)
        {
            try
            {
                //var cacheKey = $"services_{CacheTools.GenerateCacheKey(serviceFilterModel)}";

                //return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                    filter: _ => !_.IsDeleted &&
                                    (_.Name ?? "").ToLower().Trim().Contains((serviceFilterModel.Search ?? "").ToLower().Trim()),
                    include: _ => _.Include(_ => _.Packages)
                                  .Include(_ => _.ServiceAttachments)
                                  .Include(_ => _.CreatedBy),
                    pageIndex: serviceFilterModel.PageIndex,
                    pageSize: serviceFilterModel.PageSize
                );

                    var serviceModels = services.Data.Select(_ => new ServiceModel
                    {
                        Id = _.Id,
                        Name = _.Name!,
                        Description = _.Description ?? "",
                        FeedbackCount = _.FeedbackCount,
                        Rate = _.Rate,
                        MinWeight = _.MinWeight,
                        MaxWeight = _.MaxWeight,
                        Status = _.Status,
                        CategoryId = _.CategoryId,
                        ServiceAttachments = _.ServiceAttachments.ToList(),
                        Artisan = _.CreatedBy == null ? null : new AccountLiteModel
                        {
                            FirstName = _.CreatedBy.FirstName ?? "Unknown",
                            LastName = _.CreatedBy.LastName ?? "Unknown",
                            Username = _.CreatedBy.Username ?? "Unknown",
                            Email = _.CreatedBy.Email ?? "Unknown",
                            Image = _.CreatedBy.Image ?? "Unknown"
                        }
                    }).ToList();

                    var result = new Pagination<ServiceModel>(
                        serviceModels,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        serviceModels.Count
                    );
                    return new ResponseModel
                    {
                        Message = serviceModels.Any() ? "Get all services successfully" : "No services found",
                        Data = result
                    };
                //});
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Internal server error: {ex.Message}"
                };
            }

        }

        public async Task<ResponseModel> GetAllWithSuggestion(ServiceFilterModel serviceFilterModel, string sourceLanguageCode, string targetLanguageCode)
        {
            var currentUserId = _claimService.GetCurrentUserId;

            if (currentUserId.HasValue && serviceFilterModel.IsAccountSuggestion)
            {
                //var cacheKey = "suggested_services";
                //var cacheDuration = TimeSpan.FromMinutes(15);

                //var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                    var recentLogs = await _unitOfWork.UserActivityLogRepository.GetAllAsync(
                        filter: log => log.UserId == currentUserId.Value,
                        order: q => q.OrderByDescending(log => log.Timestamp),
                        pageIndex: 1,
                        pageSize: 5
                    );

                    if (!recentLogs.Data.Any())
                    {
                        var serviceList = await GetServiceListAsync(s => s.IsDeleted == false);
                        var pagedServiceList = serviceList
                            .Skip((serviceFilterModel.PageIndex - 1) * serviceFilterModel.PageSize)
                            .Take(serviceFilterModel.PageSize)
                            .ToList();
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "No recent activity found for recommendations.",
                            Data = new Pagination<ServiceModel>
                             (
                                 pagedServiceList,
                                 serviceFilterModel.PageIndex,
                                 serviceFilterModel.PageSize,
                                 pagedServiceList.Count
                             )
                        };
                    }

                    var averageEmbedding = ComputeAverageEmbedding(recentLogs.Data.Select(log => log.EmbeddingVector).ToList());
                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                        filter: _ => _.IsDeleted == false,
                        include: _ => _.Include(_ => _.Packages).Include(_ => _.ServiceAttachments).Include(_ => _.CreatedBy).Include(_ => _.Category),
                        pageIndex: 1,
                        pageSize: 10000
                     );

                    var threshold = 0.8;
                    var results = services.Data
                        .Where(s => CosineSimilarity(averageEmbedding, s.EmbeddingVector) >= threshold)
                        .Select(s => new ServiceModel
                        {
                            Id = s.Id,
                            Name = s.Name!,
                            Description = s.Description!,
                            Similarity = CosineSimilarity(averageEmbedding, s.EmbeddingVector),
                            ServiceAttachments = s.ServiceAttachments.ToList(),
                            Rate = s.Rate,
                            FeedbackCount = s.FeedbackCount,
                            Price = s.Packages.Any() ? s.Packages.Min(p => p.Price) : 0,
                            CategoryId = s.CategoryId,
                            Status = s.Status,
                            Artisan = new AccountLiteModel()
                            {
                                FirstName = s.CreatedBy.FirstName,
                                LastName = s.CreatedBy.LastName,
                                Username = s.CreatedBy.Username,
                                Email = s.CreatedBy.Email,
                                Image = s.CreatedBy.Image
                            }
                        })
                        .OrderByDescending(s => s.Similarity)
                        .ToList();

                    var pagedServices = results
                        .Skip((serviceFilterModel.PageIndex - 1) * serviceFilterModel.PageSize)
                        .Take(serviceFilterModel.PageSize)
                        .ToList();
                    if (!pagedServices.Any())
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "No similar services found.",
                            Data = null
                        };
                    }
                    var paginatedResult = new Pagination<ServiceModel>(
                        pagedServices,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        pagedServices.Count
                    );

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Get services based on user activity log successfully",
                        Data = paginatedResult
                    };
                //}, cacheDuration);

                //return responseModel;
            }
            else if (serviceFilterModel.IsEvent)
            {
                var eventDetails = _openAiService.GetEvent(sourceLanguageCode);
                if (eventDetails.Data == null)
                {
                    var serviceList = await GetServiceListAsync(s => s.IsDeleted == false);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = eventDetails!.Message,
                        Data = serviceList
                    };
                }
                ;
                var json = JsonConvert.SerializeObject(eventDetails.Data);
                var eventDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                var eventVi = eventDict["EventVi"];
                var eventEn = eventDict["EventEn"];
                var eventEmbedding = await _openAiService.GetEmbeddingAsync(new List<string> { eventVi });
                var cacheKey = "suggested_event_services";
                var cacheDuration = TimeSpan.FromDays(1);
                //var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                        filter: _ => _.IsDeleted == false,
                        include: _ => _.Include(_ => _.Packages).Include(_ => _.ServiceAttachments).Include(_ => _.CreatedBy).Include(_ => _.Category),
                        pageIndex: 1,
                        pageSize: 10000
                     );

                    var threshold = 0.8;
                    var results = services.Data
                        .Where(s => CosineSimilarity(eventEmbedding, s.EmbeddingVector) >= threshold)
                        .Select(s => new ServiceModel
                        {
                            Id = s.Id,
                            Name = s.Name!,
                            Description = s.Description!,
                            Similarity = CosineSimilarity(eventEmbedding, s.EmbeddingVector),
                            ServiceAttachments = s.ServiceAttachments.ToList(),
                            Rate = s.Rate,
                            FeedbackCount = s.FeedbackCount,
                            Price = s.Packages.Any() ? s.Packages.Min(p => p.Price) : 0,
                            CategoryId = s.CategoryId,
                            Status = s.Status,
                            Artisan = new AccountLiteModel()
                            {
                                FirstName = s.CreatedBy.FirstName,
                                LastName = s.CreatedBy.LastName,
                                Username = s.CreatedBy.Username,
                                Email = s.CreatedBy.Email,
                                Image = s.CreatedBy.Image
                            }
                        })
                        .OrderByDescending(s => s.Similarity)
                        .ToList();
                    var pagedServices = results
                         .Skip((serviceFilterModel.PageIndex - 1) * serviceFilterModel.PageSize)
                         .Take(serviceFilterModel.PageSize)
                         .ToList();
                    if (!results.Any())
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "No matching services found for the event.",
                            Data = new
                            {
                                EventName = eventDetails.Data,
                                Services = pagedServices
                            }
                        };
                    }
                    var paginatedResult = new Pagination<ServiceModel>(
                        pagedServices,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        pagedServices.Count
                    );

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Get services based on event successfully",
                        Data = new
                        {
                            EventName = eventDetails.Data,
                            Services = paginatedResult
                        }
                    };
                //}, cacheDuration);

                //return responseModel;
            }
            else
            {
                //var cacheKey = $"services_{CacheTools.GenerateCacheKey(serviceFilterModel)}";

                //var responseModel = await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
                    Guid? filterId = null;
                    if (Guid.TryParse(serviceFilterModel.IdOrUserName, out var id))
                    {
                        filterId = id;
                    }
                    var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                        filter: s =>
                            s.IsDeleted == false &&
                            (string.IsNullOrEmpty(serviceFilterModel.IdOrUserName) || (filterId.HasValue && s.CreatedById == filterId.Value)
                            || s.CreatedBy.Username.Contains(serviceFilterModel.IdOrUserName)) &&
                            (!serviceFilterModel.CategoryId.HasValue || s.Category.Id == serviceFilterModel.CategoryId ||
                            s.Category.ParentId == serviceFilterModel.CategoryId || s.Category.Parent!.ParentId == serviceFilterModel.CategoryId) &&
                            (!serviceFilterModel.SubCategoryId.HasValue || s.Category.Id == serviceFilterModel.SubCategoryId ||
                            s.Category.ParentId == serviceFilterModel.SubCategoryId) &&
                            (!serviceFilterModel.ItemId.HasValue || s.Category.Id == serviceFilterModel.ItemId) &&
                            (!serviceFilterModel.MinPrice.HasValue || s.Packages.Min(p => p.Price) >= serviceFilterModel.MinPrice) &&
                            (!serviceFilterModel.MaxPrice.HasValue || s.Packages.Min(p => p.Price) <= serviceFilterModel.MaxPrice) &&
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
                                       .Include(su => su.Category)
                                       .Include(a => a.CreatedBy),
                        pageIndex: serviceFilterModel.PageIndex,
                        pageSize: serviceFilterModel.PageSize
                    );

                    var serviceModels = services.Data.Select(s => new ServiceModel
                    {
                        Id = s.Id,
                        Name = s.Name!,
                        Description = s.Description!,
                        ServiceAttachments = s.ServiceAttachments.ToList(),
                        Rate = s.Rate,
                        FeedbackCount = s.FeedbackCount,
                        Price = s.Packages.Any() ? s.Packages.Min(p => p.Price) : 0,
                        CategoryId = s.CategoryId,
                        Status = s.Status,
                        Artisan = new AccountLiteModel()
                        {
                            FirstName = s.CreatedBy.FirstName,
                            LastName = s.CreatedBy.LastName,
                            Username = s.CreatedBy.Username,
                            Email = s.CreatedBy.Email,
                            Image = s.CreatedBy.Image,
                        }
                    }).ToList();

                    var paginatedResult = new Pagination<ServiceModel>(
                        serviceModels,
                        serviceFilterModel.PageIndex,
                        serviceFilterModel.PageSize,
                        services.TotalCount
                    );

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Get all services successfully",
                        Data = paginatedResult
                    };
                //});
                //return responseModel;
            }
        }

        private async Task<List<ServiceModel>> GetServiceListAsync(Expression<Func<Service, bool>> filter)
        {
            var services = await _unitOfWork.ServiceRepository.GetAllAsync(
                filter: filter,
                order: s => s.OrderByDescending(s => s.CreationDate),
                include: s => s.Include(p => p.Packages)
                               .ThenInclude(p => p.Orders)
                               .Include(a => a.ServiceAttachments)
                               .Include(su => su.Category)
                               .Include(a => a.CreatedBy),
                pageIndex: 1,
                pageSize: 1000
            );

            return services.Data.Select(s => new ServiceModel
            {
                Id = s.Id,
                Name = s.Name!,
                Description = s.Description!,
                ServiceAttachments = s.ServiceAttachments.ToList(),
                Rate = s.Rate,
                FeedbackCount = s.FeedbackCount,
                Price = s.Packages.Any() ? s.Packages.Min(p => p.Price) : 0,
                Artisan = new AccountLiteModel
                {
                    FirstName = s.CreatedBy.FirstName,
                    LastName = s.CreatedBy.LastName,
                    Username = s.CreatedBy.Username,
                    Email = s.CreatedBy.Email,
                    Image = s.CreatedBy.Image
                }
            }).ToList();
        }

        private float[] ComputeAverageEmbedding(List<float[]> embeddings)
        {
            if (embeddings.Count == 0) return new float[0];

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

        private static double CaculateRating(double currentRating, int currentCount, double newRating)
        {
            var result = (currentRating * currentCount + newRating) / (currentCount + 1);
            return Math.Round(result, 1);
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

        private async Task SaveSearchHistoryAsync(string searchText, Guid userId)
        {
            const int maxSearchHistory = 10;

            var searchHistories = await _unitOfWork.SearchHistoryRepository
                .GetAllAsync(filter: _ => _.CreatedById == userId);

            var existingSearchHistory = searchHistories.Data
                .FirstOrDefault(_ => _.SearchText!.Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (existingSearchHistory == null)
            {
                if (searchHistories.Data.Count() >= maxSearchHistory)
                {
                    var oldestSearchHistory = searchHistories.Data.OrderBy(_ => _.CreationDate).First();
                    _unitOfWork.SearchHistoryRepository.HardRemove(oldestSearchHistory);
                }

                await _unitOfWork.SearchHistoryRepository.AddAsync(new SearchHistory
                {
                    SearchText = searchText,
                    CreatedById = userId,
                    CreationDate = DateTime.Now
                });
            }
            else
            {
                existingSearchHistory.ModifiedById = userId;
                existingSearchHistory.CreationDate = DateTime.Now;
                _unitOfWork.SearchHistoryRepository.Update(existingSearchHistory);
            }

            await _unitOfWork.SaveChangeAsync();
        }


    }
}
