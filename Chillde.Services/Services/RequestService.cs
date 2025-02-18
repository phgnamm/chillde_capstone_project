using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Chillde.Services.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly ITranslationService _translationService;
        private readonly IStringLocalizer<OfferLanguage> _localizer;

        public RequestService(IUnitOfWork unitOfWork, IClaimService claimService,
            ICloudinaryHelper cloudinaryHelper,
            ITranslationService translationService,
            IStringLocalizer<OfferLanguage> localizer)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _translationService = translationService;
            _localizer = localizer;
        }

        public async Task<ResponseModel> Add(RequestAddModel requestAddModel, string sourceLanguageCode, string targetLanguageCode)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }
            if (requestAddModel.MinBudget > requestAddModel.MaxBudget)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "MinBudget must be less or equal to MaxBudget"
                };
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var fieldsToTranslate = new Dictionary<string, string>
                    {
                        { "Name", requestAddModel.Name },
                        { "Description", requestAddModel.Description }
                    };

                foreach (var detail in requestAddModel.RequestDetailAddModels)
                {
                    fieldsToTranslate.Add($"RequestDetail_{detail.AttributeId}_Description", detail.Description);
                }

                var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                if (translationResponse.Code != StatusCodes.Status200OK)
                {
                    throw new Exception("Failed to translate fields.");
                }

                string translatedName = translationResponse.TranslatedFields["Name"];
                string translatedDescription = translationResponse.TranslatedFields["Description"];

                var newRequest = new Request
                {
                    Id = Guid.NewGuid(),
                    CreatedById = currentUserId,
                    ItemId = requestAddModel.ItemId,
                    Name = sourceLanguageCode == "en" ? requestAddModel.Name : translatedName,
                    Description = sourceLanguageCode == "en" ? requestAddModel.Description : translatedDescription,
                    MinBudget = requestAddModel.MinBudget,
                    MaxBudget = requestAddModel.MaxBudget,
                    Timeline = requestAddModel.Timeline,
                    RequestDetails = new List<RequestDetail>()
                };

                foreach (var detail in requestAddModel.RequestDetailAddModels)
                {
                    string translatedDetailDescription = translationResponse.TranslatedFields[$"RequestDetail_{detail.AttributeId}_Description"];

                    newRequest.RequestDetails.Add(new RequestDetail
                    {
                        Id = Guid.NewGuid(),
                        AttributeId = detail.AttributeId,
                        Description = sourceLanguageCode == "en" ? detail.Description : translatedDetailDescription,
                        CreatedById = currentUserId
                    });
                }

                if (requestAddModel.Attachments != null && requestAddModel.Attachments.Any())
                {
                    var uploadTasks = requestAddModel.Attachments
                        .Where(a => a.AttachmentUrl != null)
                        .Select(async attachment => new RequestAttachment
                        {
                            Id = Guid.NewGuid(),
                            RequestId = newRequest.Id,
                            AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                                attachment.AttachmentUrl!,
                                newRequest.Id.ToString(),
                                Guid.NewGuid().ToString()),
                            AttachmentAlt = attachment.AttachmentAlt
                        })
                        .ToList();

                    var uploadedAttachments = await Task.WhenAll(uploadTasks);

                    foreach (var attachment in uploadedAttachments)
                    {
                        newRequest.RequestAttachments.Add(attachment);
                    }
                }

                await _unitOfWork.RequestRepository.AddAsync(newRequest);

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
                if (!string.IsNullOrEmpty(requestAddModel.Name))
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Request",
                        EntityId = newRequest.Id,
                        FieldName = "Name",
                        TranslationText = sourceLanguageCode != "en" ? requestAddModel.Name : translatedName,
                        LanguageId = languageId.Value
                    });
                }
                if (!string.IsNullOrEmpty(requestAddModel.Description))
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Request",
                        EntityId = newRequest.Id,
                        FieldName = "Description",
                        TranslationText = sourceLanguageCode != "en" ? requestAddModel.Description : translatedDescription,
                        LanguageId = languageId.Value
                    });
                }
                var requestDetailsList = newRequest.RequestDetails.ToList();

                foreach (var detail in requestAddModel.RequestDetailAddModels.Select((value, index) => new { value, index }))
                {
                    if (!string.IsNullOrEmpty(detail.value.Description))
                    {
                        translations.Add(new Translation
                        {
                            Id = Guid.NewGuid(),
                            EntityType = "RequestDetail",
                            EntityId = requestDetailsList[detail.index].Id,
                            FieldName = "Description",
                            TranslationText = detail.value.Description,
                            LanguageId = languageId.Value
                        });
                    }
                }

                await _unitOfWork.TranslationRepository.AddRangeAsync(translations);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Request created successfully."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Internal server error: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> GetAll(RequestFilterModel filterParameter, string sourceLanguageCode, string targetLanguage)
        {
            try
            {
                var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                var requestsResult = await _unitOfWork.RequestRepository.GetAllAsync(
                    _ => _.IsDeleted == filterParameter.IsDeleted &&
                        (string.IsNullOrEmpty(filterParameter.Search) || _.Name!.ToLower().Contains(filterParameter.Search.ToLower())),
                    requests =>
                    {
                        switch (filterParameter.Order.ToLower())
                        {
                            case "creationDate":
                                return filterParameter.OrderByDescending
                                    ? requests.OrderByDescending(request => request.CreationDate)
                                    : requests.OrderBy(request => request.CreationDate);
                            default:
                                return filterParameter.OrderByDescending
                                    ? requests.OrderByDescending(request => request.CreationDate)
                                    : requests.OrderBy(request => request.CreationDate);
                        }
                    },
                    include: requests => requests.Include(_ => _.Item),
                    pageIndex: filterParameter.PageIndex,
                    pageSize: filterParameter.PageSize
                );

                var requestIds = requestsResult.Data.Select(r => r.Id).ToList();
                if (sourceLanguageCode.ToLower() == "en")
                {
                    var requestModels = requestsResult.Data.Select(_ => new RequestModel
                    {
                        Id = _.Id,
                        Name = _.Name,
                        IsDeleted = _.IsDeleted,
                        ItemName = _.Item?.Name,
                        CreationDate = _.CreationDate,
                        MaxBudget = _.MaxBudget,
                        MinBudget = _.MinBudget,
                        Timeline = _.Timeline,
                        Description = _.Description,
                        Status = _localizer[_.Status.ToString()],
                    }).ToList();

                    var result = new Pagination<RequestModel>(requestModels, filterParameter.PageIndex,
                        filterParameter.PageSize, requestsResult.TotalCount);

                    return new ResponseModel
                    {
                        Message = "Get all requests successfully",
                        Data = result
                    };
                }
                else
                {
                    var translationFields = new[] { "Name", "Description" };
                    var translations = await _unitOfWork.TranslationRepository.GetEntitiesWithTranslationsAsync<Request, RequestModel>(
                        requestIds,
                        sourceLanguageCode,
                        request => new RequestModel
                        {
                            Id = request.Id,
                            Name = request.Name,
                            IsDeleted = request.IsDeleted,
                            ItemName = request.Item?.Name,
                            CreationDate = request.CreationDate,
                            MaxBudget = request.MaxBudget,
                            MinBudget = request.MinBudget,
                            Timeline = request.Timeline,
                            Description = request.Description,
                            Status = _localizer[request.Status.ToString()],
                        },
                        null,
                        translationFields
                    );

                    var localizedRequests = translations.Where(r => string.IsNullOrEmpty(filterParameter.Search) || r.Name!.ToLower().Contains(filterParameter.Search.ToLower())).ToList();
                    var result = new Pagination<RequestModel>(localizedRequests, filterParameter.PageIndex,
                        filterParameter.PageSize, localizedRequests.Count);

                    return new ResponseModel
                    {
                        Message = "Get all requests with translations successfully",
                        Data = result
                    };
                }
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

        public async Task<ResponseModel> GetById(Guid id, string sourceLanguageCode, string targetLanguage)
        {
            try
            {
                var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestDetails)
                                                                                             .ThenInclude(_ => _.Attribute)
                                                                                             .Include(_ => _.Item));
                if (existingRequest == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Request not found."
                    };
                }

                if (sourceLanguageCode.ToLower() != "en")
                {
                    var translationFields = new[] { "Name", "Description" };
                    var translations = await _unitOfWork.TranslationRepository.GetEntitiesWithTranslationsAsync<Request, RequestGetByIdModel>(
                        new List<Guid> { id },
                        sourceLanguageCode,
                        request => new RequestGetByIdModel
                        {
                            Id = request.Id,
                            Name = request.Name!,
                            Description = request.Description!,
                            MinBudget = request.MinBudget ?? 0,
                            MaxBudget = request.MaxBudget ?? 0,
                            Timeline = request.Timeline ?? 0,
                            Attachments = request.RequestAttachments
                            .Select(att => new AttachmentGetModel
                            {
                                AttachmentUrl = att.AttachmentUrl,
                                AttachmentAlt = att.AttachmentAlt
                            }).ToList(),
                            Status = request.Status,
                            ItemId = request.ItemId,
                            ItemName = _localizer[request.Item.Name!.ToString()],
                            ItemCode = request.Item?.Code!,
                            ItemImageUrl = request.Item?.ImageUrl!,
                            RequestDetailGetByIdModels = request.RequestDetails.Select(detail => new RequestDetailGetByIdModel
                            {
                                Id = detail.Id,
                                Description = detail.Description,
                                ItemAttributeId = detail.AttributeId,
                                ItemAttributeName = detail.Attribute?.Name!
                            }).ToList()
                        },
                        "RequestDetails",
                        translationFields,
                        nestedRelationships: new[] { "Attribute", "RequestAttachments" },
                        "Description"
                    );

                    if (translations != null)
                    {
                        var updatedTranslations = translations.Select(translation =>
                        {
                            translation.RequestDetailGetByIdModels = translation.RequestDetailGetByIdModels!
                                .Select(detail => new RequestDetailGetByIdModel
                                {
                                    Id = detail.Id,
                                    Description = detail.Description,
                                    ItemAttributeId = existingRequest.RequestDetails.FirstOrDefault()!.Attribute.Id,
                                    ItemAttributeName = _localizer[existingRequest.RequestDetails.FirstOrDefault()!.Attribute.Name.ToString()]
                                }).ToList();
                            return translation;
                        }).ToList();

                        return new ResponseModel
                        {
                            Code = StatusCodes.Status200OK,
                            Data = updatedTranslations,
                            Message = "Get request detail successfully"
                        };
                    }
                }

                var requestModel = new RequestGetByIdModel
                {
                    Id = existingRequest.Id,
                    Name = existingRequest.Name ?? "Unknown",
                    Description = existingRequest.Description ?? "Unknown",
                    MinBudget = existingRequest.MinBudget ?? 0,
                    MaxBudget = existingRequest.MaxBudget ?? 0,
                    Timeline = existingRequest.Timeline ?? 0,
                    Attachments = existingRequest.RequestAttachments
                    .Select(att => new AttachmentGetModel
                    {
                        AttachmentUrl = att.AttachmentUrl,
                        AttachmentAlt = att.AttachmentAlt ?? "No description"
                    }).ToList(),
                    Status = existingRequest.Status,
                    ItemId = existingRequest.ItemId,
                    ItemName = existingRequest.Item?.Name ?? "Unknown",
                    ItemCode = existingRequest.Item?.Code ?? "Unknown",
                    ItemImageUrl = existingRequest.Item?.ImageUrl ?? "Unknown",
                    RequestDetailGetByIdModels = existingRequest.RequestDetails.Select(_ => new RequestDetailGetByIdModel
                    {
                        Id = _.Id,
                        Description = _.Description,
                        ItemAttributeId = _.AttributeId,
                        ItemAttributeName = _.Attribute?.Name ?? "Unknown"
                    }).ToList()
                };

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Data = requestModel,
                    Message = "Get request detail success"
                };
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

        public async Task<ResponseModel> Update(Guid id, RequestUpdateModel requestUpdateModel, string sourceLanguageCode, string targetLanguageCode)
        {
            if (requestUpdateModel == null || id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input."
                };
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestDetails));
                if (existingRequest == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Request not found."
                    };
                }
                var languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode != "en" ? targetLanguageCode : sourceLanguageCode);
                var translationName = await _unitOfWork.TranslationRepository.GetTranslationAsync("Request", id, "Name", languageId);
                var translationDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync("Request", id, "Description", languageId);
                bool changesMade = false;

                if (!string.Equals(requestUpdateModel.Name, existingRequest.Name, StringComparison.OrdinalIgnoreCase) &&
                   (translationName != null && !string.Equals(requestUpdateModel.Name, translationName.TranslationText, StringComparison.OrdinalIgnoreCase)))
                {
                    var translationResponse = await _translationService.TranslateAsync(requestUpdateModel.Name!, sourceLanguageCode, targetLanguageCode);
                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        throw new Exception("Failed to translate Name.");
                    }
                    translationName!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : requestUpdateModel.Name!;
                    existingRequest.Name = targetLanguageCode != "en" ? requestUpdateModel.Name : translationResponse.Message;
                    _unitOfWork.TranslationRepository.Update(translationName);
                    changesMade = true;
                }

                if (!string.Equals(requestUpdateModel.Description, existingRequest.Description, StringComparison.OrdinalIgnoreCase) &&
                   (translationDescription != null && !string.Equals(requestUpdateModel.Description, translationDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
                {
                    var translationResponse = await _translationService.TranslateAsync(requestUpdateModel.Description!, sourceLanguageCode, targetLanguageCode);
                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        throw new Exception("Failed to translate Description.");
                    }

                    translationDescription!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : requestUpdateModel.Description!;
                    existingRequest.Description = targetLanguageCode != "en" ? requestUpdateModel.Description : translationResponse.Message;
                    _unitOfWork.TranslationRepository.Update(translationDescription);
                    changesMade = true;
                }

                foreach (var detailModel in requestUpdateModel.RequestDetailUpdateModels!)
                {
                    var translationRequestDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync(
                        "RequestDetail",
                        detailModel.Id,
                        "Description",
                        languageId
                    );

                    var existingDetail = existingRequest.RequestDetails.FirstOrDefault(rd => rd.Id == detailModel.Id);
                    if (existingDetail == null)
                    {
                        continue;
                    }
                    if (!string.Equals(detailModel.Description, existingDetail.Description, StringComparison.OrdinalIgnoreCase) &&
                        (translationRequestDescription != null && !string.Equals(detailModel.Description, translationRequestDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
                    {
                        var translationResponse = await _translationService.TranslateAsync(detailModel.Description!, sourceLanguageCode, targetLanguageCode);
                        if (translationResponse.Code != StatusCodes.Status200OK)
                        {
                            throw new Exception($"Failed to translate Description for RequestDetail ID: {detailModel.Id}");
                        }
                        existingDetail.Description = targetLanguageCode != "en" ? detailModel.Description : translationResponse.Message;

                        if (translationRequestDescription != null)
                        {
                            translationRequestDescription.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : detailModel.Description!;
                            _unitOfWork.TranslationRepository.Update(translationRequestDescription);
                        }

                        changesMade = true;
                    }
                    if (existingDetail.AttributeId != detailModel.AttributeId)
                    {
                        existingDetail.AttributeId = detailModel.AttributeId;
                        changesMade = true;
                    }
                }


                if (requestUpdateModel.MinBudget.HasValue && existingRequest.MinBudget != requestUpdateModel.MinBudget.Value)
                {
                    existingRequest.MinBudget = requestUpdateModel.MinBudget.Value;
                    changesMade = true;
                }

                if (requestUpdateModel.MaxBudget.HasValue && existingRequest.MaxBudget != requestUpdateModel.MaxBudget.Value)
                {
                    existingRequest.MaxBudget = requestUpdateModel.MaxBudget.Value;
                    changesMade = true;
                }

                if (requestUpdateModel.Attachments != null && requestUpdateModel.Attachments.Any())
                {
                    var uploadTasks = new List<Task<RequestAttachment>>();
                    foreach (var attachment in requestUpdateModel.Attachments)
                    {
                        if (attachment.AttachmentUrl != null)
                        {
                            uploadTasks.Add(
                                Task.Run(async () =>
                                {
                                    var attachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                                        attachment.AttachmentUrl,
                                        existingRequest.Id.ToString(),
                                        Guid.NewGuid().ToString()
                                    );

                                    return new RequestAttachment
                                    {
                                        Id = Guid.NewGuid(),
                                        RequestId = existingRequest.Id,
                                        AttachmentUrl = attachmentUrl,
                                        AttachmentAlt = attachment.AttachmentAlt
                                    };
                                })
                            );
                        }
                    }
                    var uploadedAttachments = await Task.WhenAll(uploadTasks);
                    foreach (var attachment in uploadedAttachments)
                    {
                        existingRequest.RequestAttachments.Add(attachment);
                    }

                    changesMade = true;
                }
                if (!changesMade)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
                    };
                }
                _unitOfWork.RequestRepository.Update(existingRequest);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Request updated successfully."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}
