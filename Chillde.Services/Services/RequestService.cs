using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.RequestModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.SubcategoryModels;
using Chillde.Services.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Nest;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Mail;
using System.Net.WebSockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Chillde.Services.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly ITranslationService _translationService;
        private readonly IStringLocalizer<OfferLanguage> _localizer;
        private readonly IBadWordFilterService _badWordFilterService;

        public RequestService(IUnitOfWork unitOfWork, IClaimService claimService,
            ICloudinaryHelper cloudinaryHelper,
            ITranslationService translationService,
            IStringLocalizer<OfferLanguage> localizer,
            IBadWordFilterService badWordFilterService

            )
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _translationService = translationService;
            _localizer = localizer;
            _badWordFilterService = badWordFilterService;
        }

        public async Task<ResponseModel> Add(RequestAddModel requestAddModel, string sourceLanguageCode, string targetLanguageCode)
        {
            //var currentUserId = _claimService.GetCurrentUserId;
            //if (!currentUserId.HasValue)
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status401Unauthorized,
            //        Message = "Unauthorized"
            //    };
            //}
            //if (requestAddModel.MinBudget > requestAddModel.MaxBudget)
            //{
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status400BadRequest,
            //        Message = "MinBudget must be less or equal to MaxBudget"
            //    };
            //}
            //await _unitOfWork.BeginTransactionAsync();

            //try
            //{
            //    var fieldsToTranslate = new Dictionary<string, string>
            //        {
            //            { "Name", requestAddModel.Name },
            //            { "Description", requestAddModel.Description }
            //        };

            //    foreach (var detail in requestAddModel.RequestDetailAddModels)
            //    {
            //        fieldsToTranslate.Add($"RequestDetail_{detail.RequestAttributeId}_Description", detail.Description);
            //    }

            //    var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
            //    if (translationResponse.Code != StatusCodes.Status200OK)
            //    {
            //        throw new Exception("Failed to translate fields.");
            //    }

            //    string translatedName = translationResponse.TranslatedFields["Name"];
            //    string translatedDescription = translationResponse.TranslatedFields["Description"];

            //    var newRequest = new Request
            //    {
            //        Id = Guid.NewGuid(),
            //        CreatedById = currentUserId,
            //        ItemId = requestAddModel.ItemId,
            //        Name = sourceLanguageCode == "en" ? requestAddModel.Name : translatedName,
            //        Description = sourceLanguageCode == "en" ? requestAddModel.Description : translatedDescription,
            //        MinBudget = requestAddModel.MinBudget,
            //        MaxBudget = requestAddModel.MaxBudget,
            //        Timeline = requestAddModel.Timeline,
            //        //RequestDetails = new List<RequestDetail>()
            //    };

            //    foreach (var detail in requestAddModel.RequestDetailAddModels)
            //    {
            //        string translatedDetailDescription = translationResponse.TranslatedFields[$"RequestDetail_{detail.RequestAttributeId}_Description"];

            //        //newRequest.RequestDetails.Add(new RequestDetail
            //        //{
            //        //    Id = Guid.NewGuid(),
            //        //    RequestAttributeId = detail.RequestAttributeId,
            //        //    Description = sourceLanguageCode == "en" ? detail.Description : translatedDetailDescription,
            //        //    CreatedById = currentUserId
            //        //});
            //    }

            //    if (requestAddModel.Attachments != null && requestAddModel.Attachments.Any())
            //    {
            //        var uploadTasks = requestAddModel.Attachments
            //            .Where(a => a.AttachmentUrl != null)
            //            .Select(async attachment => new RequestAttachment
            //            {
            //                Id = Guid.NewGuid(),
            //                RequestId = newRequest.Id,
            //                AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
            //                    attachment.AttachmentUrl!,
            //                    newRequest.Id.ToString(),
            //                    Guid.NewGuid().ToString()),
            //                AttachmentAlt = attachment.AttachmentAlt
            //            })
            //            .ToList();

            //        var uploadedAttachments = await Task.WhenAll(uploadTasks);

            //        foreach (var attachment in uploadedAttachments)
            //        {
            //            newRequest.RequestAttachments.Add(attachment);
            //        }
            //    }

            //    await _unitOfWork.RequestRepository.AddAsync(newRequest);

            //    var translations = new List<Translation>();
            //    Guid? languageId = null;
            //    if (sourceLanguageCode != "en")
            //    {
            //        languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(sourceLanguageCode);
            //    }
            //    else
            //    {
            //        languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode);
            //    }
            //    if (!string.IsNullOrEmpty(requestAddModel.Name))
            //    {
            //        translations.Add(new Translation
            //        {
            //            Id = Guid.NewGuid(),
            //            EntityType = "Request",
            //            EntityId = newRequest.Id,
            //            FieldName = "Name",
            //            TranslationText = sourceLanguageCode != "en" ? requestAddModel.Name : translatedName,
            //            LanguageId = languageId.Value
            //        });
            //    }
            //    if (!string.IsNullOrEmpty(requestAddModel.Description))
            //    {
            //        translations.Add(new Translation
            //        {
            //            Id = Guid.NewGuid(),
            //            EntityType = "Request",
            //            EntityId = newRequest.Id,
            //            FieldName = "Description",
            //            TranslationText = sourceLanguageCode != "en" ? requestAddModel.Description : translatedDescription,
            //            LanguageId = languageId.Value
            //        });
            //    }
            //    var requestDetailsList = newRequest.RequestAttributes.ToList();

            //    foreach (var detail in requestAddModel.RequestDetailAddModels.Select((value, index) => new { value, index }))
            //    {
            //        if (!string.IsNullOrEmpty(detail.value.Description))
            //        {
            //            translations.Add(new Translation
            //            {
            //                Id = Guid.NewGuid(),
            //                EntityType = "RequestDetail",
            //                EntityId = requestDetailsList[detail.index].Id,
            //                FieldName = "Description",
            //                TranslationText = detail.value.Description,
            //                LanguageId = languageId.Value
            //            });
            //        }
            //    }

            //    await _unitOfWork.TranslationRepository.AddRangeAsync(translations);
            //    await _unitOfWork.SaveChangeAsync();
            //    await _unitOfWork.CommitTransactionAsync();

            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status201Created,
            //        Message = "Request created successfully."
            //    };
            //}
            //catch (Exception ex)
            //{
            //    await _unitOfWork.RollbackTransactionAsync();
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status500InternalServerError,
            //        Message = $"Internal server error: {ex.Message}"
            //    };
            //}
            return null;
        }

        public async Task<ResponseModel> AddAsync(RequestAddModel requestAddModel, string sourceLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { requestAddModel.Name, requestAddModel.Description };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code != StatusCodes.Status200OK)
                        return response;
                }

                var currentUserId = _claimService.GetCurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized"
                    };
                }

                if (requestAddModel.MaxBudget < requestAddModel.MinBudget)
                {
                    return new ResponseModel { Message = "MaxBudget must be greater than or equal to MinBudget.", Code = StatusCodes.Status400BadRequest };
                }

                var newRequest = CreateNewRequest(requestAddModel, currentUserId.Value);

                await _unitOfWork.RequestRepository.AddAsync(newRequest);
                await ProcessAttachments((List<RequestAttachmentAddModel>)requestAddModel.RequestAttachmentAddModels, (List<RequestAttachment>)newRequest.RequestAttachments);
                await ProcessAttributes((List<RequestAttributeAddModel>)requestAddModel.RequestAttributeAddModels, (List<RequestAttribute>)newRequest.RequestAttributes);
                var result = await _unitOfWork.SaveChangeAsync();

                return result > 0 ? new ResponseModel { Message = "Create request successfully" } : new ResponseModel { Message = "Create request unsuccessfully", Code = StatusCodes.Status400BadRequest };
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
                    include: null,
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

        //public async Task<ResponseModel> GetById(Guid id, string sourceLanguageCode, string targetLanguage)
        //{
        //    try
        //    {
        //        var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
        //        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        //        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        //        var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestAttributes)
        //                                                                                     .ThenInclude(_ => _.RequestAttributeAttachments)
        //                                                                                     .Include(_ => _.Item));
        //        if (existingRequest == null)
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status404NotFound,
        //                Message = "Request not found."
        //            };
        //        }

        //        if (sourceLanguageCode.ToLower() != "en")
        //        {
        //            var translationFields = new[] { "Name", "Description" };
        //            var translations = await _unitOfWork.TranslationRepository.GetEntitiesWithTranslationsAsync<Request, RequestGetByIdModel>(
        //                new List<Guid> { id },
        //                sourceLanguageCode,
        //                request => new RequestGetByIdModel
        //                {
        //                    Id = request.Id,
        //                    Name = request.Name!,
        //                    Description = request.Description!,
        //                    MinBudget = request.MinBudget ?? 0,
        //                    MaxBudget = request.MaxBudget ?? 0,
        //                    Timeline = request.Timeline ?? 0,
        //                    Attachments = request.RequestAttachments
        //                    .Select(att => new AttachmentGetModel
        //                    {
        //                        AttachmentUrl = att.AttachmentUrl,
        //                        AttachmentAlt = att.AttachmentAlt
        //                    }).ToList(),
        //                    Status = request.Status,
        //                    ItemId = request.ItemId,
        //                    ItemName = _localizer[request.Item.Name!.ToString()],
        //                    ItemCode = request.Item?.Code!,
        //                    ItemImageUrl = request.Item?.ImageUrl!,
        //                    RequestDetailGetByIdModels = request.RequestAttributes.Select(detail => new RequestDetailGetByIdModel
        //                    {
        //                        Id = detail.Id,
        //                        Name = detail.Name,
        //                    }).ToList()
        //                },
        //                "RequestDetails",
        //                translationFields,
        //                nestedRelationships: new[] { "Attribute", "RequestAttachments" },
        //                "Description"
        //            );

        //            if (translations != null)
        //            {
        //                var updatedTranslations = translations.Select(translation =>
        //                {
        //                    translation.RequestDetailGetByIdModels = translation.RequestDetailGetByIdModels!
        //                        .Select(detail => new RequestDetailGetByIdModel
        //                        {
        //                            Id = detail.Id,
        //                            Name = detail.Name,
        //                        }).ToList();
        //                    return translation;
        //                }).ToList();

        //                return new ResponseModel
        //                {
        //                    Code = StatusCodes.Status200OK,
        //                    Data = updatedTranslations,
        //                    Message = "Get request detail successfully"
        //                };
        //            }
        //        }

        //        var requestModel = new RequestGetByIdModel
        //        {
        //            Id = existingRequest.Id,
        //            Name = existingRequest.Name ?? "Unknown",
        //            Description = existingRequest.Description ?? "Unknown",
        //            MinBudget = existingRequest.MinBudget ?? 0,
        //            MaxBudget = existingRequest.MaxBudget ?? 0,
        //            Timeline = existingRequest.Timeline ?? 0,
        //            Attachments = existingRequest.RequestAttachments
        //            .Select(att => new AttachmentGetModel
        //            {
        //                AttachmentUrl = att.AttachmentUrl,
        //                AttachmentAlt = att.AttachmentAlt ?? "No description"
        //            }).ToList(),
        //            Status = existingRequest.Status,
        //            ItemId = existingRequest.ItemId,
        //            ItemName = existingRequest.Item?.Name ?? "Unknown",
        //            ItemCode = existingRequest.Item?.Code ?? "Unknown",
        //            ItemImageUrl = existingRequest.Item?.ImageUrl ?? "Unknown",
        //            RequestDetailGetByIdModels = existingRequest.RequestAttributes.Select(_ => new RequestDetailGetByIdModel
        //            {
        //                Id = _.Id,
        //                Name = _.Name,
        //            }).ToList()
        //        };

        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status200OK,
        //            Data = requestModel,
        //            Message = "Get request detail success"
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = $"Internal server error: {ex.Message}"
        //        };
        //    }
        //}

        //public async Task<ResponseModel> Update(Guid id, RequestUpdateModel requestUpdateModel, string sourceLanguageCode, string targetLanguageCode)
        //{
        //    if (requestUpdateModel == null || id == Guid.Empty)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Invalid input."
        //        };
        //    }

        //    await _unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestAttributes));
        //        if (existingRequest == null)
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status404NotFound,
        //                Message = "Request not found."
        //            };
        //        }
        //        var languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode != "en" ? targetLanguageCode : sourceLanguageCode);
        //        var translationName = await _unitOfWork.TranslationRepository.GetTranslationAsync("Request", id, "Name", languageId);
        //        var translationDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync("Request", id, "Description", languageId);
        //        bool changesMade = false;

        //        if (!string.Equals(requestUpdateModel.Name, existingRequest.Name, StringComparison.OrdinalIgnoreCase) &&
        //           (translationName != null && !string.Equals(requestUpdateModel.Name, translationName.TranslationText, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            var translationResponse = await _translationService.TranslateAsync(requestUpdateModel.Name!, sourceLanguageCode, targetLanguageCode);
        //            if (translationResponse.Code != StatusCodes.Status200OK)
        //            {
        //                throw new Exception("Failed to translate Name.");
        //            }
        //            translationName!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : requestUpdateModel.Name!;
        //            existingRequest.Name = targetLanguageCode != "en" ? requestUpdateModel.Name : translationResponse.Message;
        //            _unitOfWork.TranslationRepository.Update(translationName);
        //            changesMade = true;
        //        }

        //        if (!string.Equals(requestUpdateModel.Description, existingRequest.Description, StringComparison.OrdinalIgnoreCase) &&
        //           (translationDescription != null && !string.Equals(requestUpdateModel.Description, translationDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            var translationResponse = await _translationService.TranslateAsync(requestUpdateModel.Description!, sourceLanguageCode, targetLanguageCode);
        //            if (translationResponse.Code != StatusCodes.Status200OK)
        //            {
        //                throw new Exception("Failed to translate Description.");
        //            }

        //            translationDescription!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : requestUpdateModel.Description!;
        //            existingRequest.Description = targetLanguageCode != "en" ? requestUpdateModel.Description : translationResponse.Message;
        //            _unitOfWork.TranslationRepository.Update(translationDescription);
        //            changesMade = true;
        //        }

        //        foreach (var detailModel in requestUpdateModel.RequestDetailUpdateModels!)
        //        {
        //            var translationRequestDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync(
        //                "RequestDetail",
        //                detailModel.Id,
        //                "Description",
        //                languageId
        //            );

        //            var existingDetail = existingRequest.RequestAttributes.FirstOrDefault(rd => rd.Id == detailModel.Id);
        //            if (existingDetail == null)
        //            {
        //                continue;
        //            }
        //            if (!string.Equals(detailModel.Description, existingDetail.Name, StringComparison.OrdinalIgnoreCase) &&
        //                (translationRequestDescription != null && !string.Equals(detailModel.Description, translationRequestDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                var translationResponse = await _translationService.TranslateAsync(detailModel.Description!, sourceLanguageCode, targetLanguageCode);
        //                if (translationResponse.Code != StatusCodes.Status200OK)
        //                {
        //                    throw new Exception($"Failed to translate Description for RequestDetail ID: {detailModel.Id}");
        //                }
        //                existingDetail.Name = targetLanguageCode != "en" ? detailModel.Description : translationResponse.Message;

        //                if (translationRequestDescription != null)
        //                {
        //                    translationRequestDescription.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : detailModel.Description!;
        //                    _unitOfWork.TranslationRepository.Update(translationRequestDescription);
        //                }

        //                changesMade = true;
        //            }
        //        }


        //        if (requestUpdateModel.MinBudget.HasValue && existingRequest.MinBudget != requestUpdateModel.MinBudget.Value)
        //        {
        //            existingRequest.MinBudget = requestUpdateModel.MinBudget.Value;
        //            changesMade = true;
        //        }

        //        if (requestUpdateModel.MaxBudget.HasValue && existingRequest.MaxBudget != requestUpdateModel.MaxBudget.Value)
        //        {
        //            existingRequest.MaxBudget = requestUpdateModel.MaxBudget.Value;
        //            changesMade = true;
        //        }

        //        //if (requestUpdateModel.Attachments != null && requestUpdateModel.Attachments.Any())
        //        //{
        //        //    var uploadTasks = new List<Task<RequestAttachment>>();
        //        //    foreach (var attachment in requestUpdateModel.Attachments)
        //        //    {
        //        //        if (attachment.AttachmentUrl != null)
        //        //        {
        //        //            uploadTasks.Add(
        //        //                Task.Run(async () =>
        //        //                {
        //        //                    var attachmentUrl = await _cloudinaryHelper.UploadImageAsync(
        //        //                        attachment.AttachmentUrl,
        //        //                        existingRequest.Id.ToString(),
        //        //                        Guid.NewGuid().ToString()
        //        //                    );

        //        //                    return new RequestAttachment
        //        //                    {
        //        //                        Id = Guid.NewGuid(),
        //        //                        RequestId = existingRequest.Id,
        //        //                        AttachmentUrl = attachmentUrl,
        //        //                        AttachmentAlt = attachment.AttachmentAlt
        //        //                    };
        //        //                })
        //        //            );
        //        //        }
        //        //    }
        //        //    var uploadedAttachments = await Task.WhenAll(uploadTasks);
        //        //    foreach (var attachment in uploadedAttachments)
        //        //    {
        //        //        existingRequest.RequestAttachments.Add(attachment);
        //        //    }

        //        //    changesMade = true;
        //        //}
        //        if (!changesMade)
        //        {
        //            await _unitOfWork.RollbackTransactionAsync();
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status204NoContent,
        //                Message = "No changes detected."
        //            };
        //        }
        //        _unitOfWork.RequestRepository.Update(existingRequest);
        //        await _unitOfWork.SaveChangeAsync();
        //        await _unitOfWork.CommitTransactionAsync();

        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status200OK,
        //            Message = "Request updated successfully."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = $"Error: {ex.Message}"
        //        };
        //    }
        //}

        #region Create New Request
        private Request CreateNewRequest(RequestAddModel model, Guid userId)
        {
            return new Request
            {
                Name = model.Name,
                Description = model.Description,
                MinBudget = model.MinBudget,
                MaxBudget = model.MaxBudget,
                Timeline = model.Timeline,
                CreatedById = userId,
                RequestAttributes = new List<RequestAttribute>(),
                RequestAttachments = new List<RequestAttachment>()
            };
        }

        private async Task ProcessAttachments(List<RequestAttachmentAddModel> attachmentModels, List<RequestAttachment> requestAttachments)
        {
            if (attachmentModels == null) return;

            foreach (var attachment in attachmentModels.Where(_ => _.AttachmentUrl != null))
            {
                if (string.IsNullOrEmpty(attachment.AttachmentAlt))
                {
                    throw new Exception("AttachmentAlt is empty");
                }
                var uploadedUrl = await UploadFile(attachment.AttachmentUrl, FolderAttachment.REQUEST);
                requestAttachments.Add(new RequestAttachment
                {
                    AttachmentUrl = uploadedUrl,
                    AttachmentAlt = attachment.AttachmentAlt
                });
            }
        }

        private async Task ProcessAttributes(List<RequestAttributeAddModel> attributeModels, List<RequestAttribute> requestAttributes)
        {
            if (attributeModels == null) return;

            foreach (var attribute in attributeModels)
            {
                var newAttribute = new RequestAttribute
                {
                    Name = attribute.Name,
                    Type = attribute.Type,
                    RequestAttributeValues = new List<RequestAttributeValue>(),
                    RequestAttributeAttachments = new List<RequestAttributeAttachment>()
                };

                if (attribute.Type == ItemAttributeType.File)
                {
                    if (attribute.RequestAttributeAttachmentAddModels == null || !attribute.RequestAttributeAttachmentAddModels.Any())
                    {
                        throw new Exception("Each attribute must have at least one value");
                    }
                    await ProcessAttributeAttachments((List<RequestAttributeAttachmentAddModel>)attribute.RequestAttributeAttachmentAddModels, (List<RequestAttributeAttachment>)newAttribute.RequestAttributeAttachments);
                }
                else
                {
                    if (attribute.RequestAttributeValueAddModels == null || !attribute.RequestAttributeValueAddModels.Any())
                    {
                        throw new Exception("Each attribute must have at least one value");
                    }
                    ProcessAttributeValues((List<RequestAttributeValueAddModel>)attribute.RequestAttributeValueAddModels, (List<RequestAttributeValue>)newAttribute.RequestAttributeValues);
                }

                requestAttributes.Add(newAttribute);
            }
        }

        private async Task ProcessAttributeAttachments(List<RequestAttributeAttachmentAddModel> attachmentModels, List<RequestAttributeAttachment> attributeAttachments)
        {
            foreach (var attachment in attachmentModels.Where(_ => _.AttachmentUrl != null))
            {
                if (string.IsNullOrEmpty(attachment.AttachmentAlt))
                {
                    throw new Exception("AttachmentAlt is empty");
                }

     
                var uploadedUrl = await UploadFile(attachment.AttachmentUrl, FolderAttachment.REQUESTATTRIBUTE);

                attributeAttachments.Add(new RequestAttributeAttachment
                {
                    AttachmentUrl = uploadedUrl,
                    AttachmentAlt = attachment.AttachmentAlt
                });
            }
        }

        private void ProcessAttributeValues(List<RequestAttributeValueAddModel> valueModels, List<RequestAttributeValue> attributeValues)
        {
            int intOrder = 0;
            foreach (var valueModel in valueModels)
            {
                attributeValues.Add(new RequestAttributeValue
                {
                    Value = JsonConvert.SerializeObject(valueModel.Value),
                    IntOrder = intOrder++
                });
            }
        }
        #endregion

        #region Get Request Detail
        public async Task<ResponseModel> GetByIdAsync(Guid id)
        {
            var request = await _unitOfWork.RequestRepository.GetAsync(id, include: _ => _.Include(_ => _.RequestAttributes).ThenInclude(_ => _.RequestAttributeValues).Include(_ => _.RequestAttributes).ThenInclude(_ => _.RequestAttributeAttachments).Include(_ => _.RequestAttachments));
            var requestModel = new RequestGetByIdModel
            {
                Id = request.Id,
                Name = request.Name ?? "Unkown",
                Description = request.Description ?? "Unkown",
                MinBudget = (decimal)request.MinBudget,
                MaxBudget = (decimal)request.MaxBudget,
                Timeline = (int)request.Timeline,
                Status = request.Status,
                RequestAttachmentGetModels = request?.RequestAttachments?.Select(_ => new RequestAttachmentGetModel
                {
                    Id = _.Id,
                    AttachmentAlt = _.AttachmentAlt,
                    AttachmentUrl = _.AttachmentUrl,
                }).ToList(),
                RequestAttributeGetModels = request.RequestAttributes.Select(_ => new RequestAttributeGetModel
                {
                    Id = _.Id,
                    Name = _.Name,
                    Type = _.Type,
                    RequestAttributeValueGetModels = _?.RequestAttributeValues?.Select(_ => new RequestAttributeValueGetModel
                    {
                        Id = _.Id,
                        Value = (string)JsonConvert.DeserializeObject(_.Value),
                    }).ToList(),
                    RequestAttributeAttachmentGetModels = _?.RequestAttributeAttachments?.Select(_ => new RequestAttributeAttachmentGetModel
                    {
                        Id = _.Id,
                        AttachmentAlt = _.AttachmentAlt,
                        AttachmentUrl = _.AttachmentUrl
                    }).ToList()
                }).ToList()

            };
            return new ResponseModel { Data = requestModel };
        }
        #endregion

        #region Update Request
        public async Task<ResponseModel> UpdateRequestAsync(Guid requestId, RequestUpdateModel requestUpdateModel)
        {
            try
            {
                var request = await _unitOfWork.RequestRepository.GetAsync(requestId,
                    include: _ => _.Include(_ => _.RequestAttributes)
                                   .ThenInclude(_ => _.RequestAttributeValues)
                                   .Include(_ => _.RequestAttributes)
                                   .ThenInclude(_ => _.RequestAttributeAttachments)
                                   .Include(_ => _.RequestAttachments));

                if (request == null)
                {
                    return new ResponseModel { Message = "Request not found.", Code = StatusCodes.Status404NotFound };
                }

                request.MinBudget = requestUpdateModel.MinBudget ?? request.MinBudget;
                request.MaxBudget = requestUpdateModel.MaxBudget ?? request.MaxBudget;
                request.Timeline = requestUpdateModel.Timeline ?? request.Timeline;

                if (requestUpdateModel.RequestAttributes != null)
                {
                    await UpdateRequestAttributes(request, (List<RequestAttributeUpdateModel>)requestUpdateModel.RequestAttributes);
                }

                if (requestUpdateModel.RequestAttachments != null)
                {
                    await UpdateRequestAttachments(request, (List<RequestAttachmentUpdateModel>)requestUpdateModel.RequestAttachments);
                }

                await _unitOfWork.SaveChangeAsync();
                return new ResponseModel { Message = "Request updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Message = $"An error occurred: {ex.Message}", Code = StatusCodes.Status500InternalServerError };
            }
        }
        private async Task UpdateRequestAttributes(Request request, List<RequestAttributeUpdateModel> attributes)
        {
            foreach (var attributeModel in attributes)
            {
                var existingAttribute = request.RequestAttributes.FirstOrDefault(_ => _.Id == attributeModel.Id);

                if (existingAttribute != null)
                {
                    await UpdateExistingAttribute(existingAttribute, attributeModel);
                }
                else if (!string.IsNullOrEmpty(attributeModel.Name))
                {
                    var newAttribute = new RequestAttribute
                    {
                        Name = attributeModel.Name,
                        Type = (ItemAttributeType)attributeModel.Type,
                        RequestId = request.Id,
                        RequestAttributeValues = new List<RequestAttributeValue>(),
                        RequestAttributeAttachments = new List<RequestAttributeAttachment>()
                    };

                    if (newAttribute.Type != ItemAttributeType.File)
                    {
                        AddAttributeValues(newAttribute, (List<RequestAttributeValueUpdateModel>)attributeModel.RequestAttributeValueAddModels);
                    }
                    else
                    {
                        await AddAttributeAttachments(newAttribute, (List<RequestAttributeAttachmentUpdateModel>)attributeModel.RequestAttributeAttachmentAddModels);
                    }

                    request.RequestAttributes.Add(newAttribute);
                }
            }
        }
        private async Task UpdateExistingAttribute(RequestAttribute existingAttribute, RequestAttributeUpdateModel attributeModel)
        {
            if (existingAttribute.Type != ItemAttributeType.File && attributeModel.RequestAttributeValueAddModels != null)
            {
                AddAttributeValues(existingAttribute, (List<RequestAttributeValueUpdateModel>)attributeModel.RequestAttributeValueAddModels);
            }
            else if (existingAttribute.Type == ItemAttributeType.File && attributeModel.RequestAttributeAttachmentAddModels != null)
            {
                await AddOrUpdateAttributeAttachments(existingAttribute, (List<RequestAttributeAttachmentUpdateModel>)attributeModel.RequestAttributeAttachmentAddModels);
            }
        }
        private void AddAttributeValues(RequestAttribute attribute, List<RequestAttributeValueUpdateModel> values)
        {
            foreach (var valueModel in values)
            {
                if (!string.IsNullOrEmpty(valueModel.Value))
                {
                    var existingValue = attribute.RequestAttributeValues.FirstOrDefault(_ => _.Id == valueModel.Id);

                    if (existingValue != null)
                    {
                        existingValue.Value = valueModel.Value;
                    }
                    else
                    {
                        attribute.RequestAttributeValues.Add(new RequestAttributeValue
                        {
                            Value = JsonConvert.SerializeObject(valueModel.Value),
                            IntOrder = attribute.RequestAttributeValues.Any() ? attribute.RequestAttributeValues.Max(_ => _.IntOrder) + 1 : 1
                        });
                    }
                }
            }
        }
        private async Task AddOrUpdateAttributeAttachments(RequestAttribute attribute, List<RequestAttributeAttachmentUpdateModel> attachments)
        {
            foreach (var attachmentModel in attachments)
            {
                var existingAttachment = attribute.RequestAttributeAttachments.FirstOrDefault(_ => _.Id == attachmentModel.Id);

                if (existingAttachment != null)
                {
                    existingAttachment.AttachmentUrl = attachmentModel.AttachmentUrl != null
                        ? await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.REQUESTATTRIBUTE)
                        : existingAttachment.AttachmentUrl;

                    existingAttachment.AttachmentAlt = attachmentModel.AttachmentAlt ?? existingAttachment.AttachmentAlt;
                }
                else
                {
                    await AddAttributeAttachments(attribute, new List<RequestAttributeAttachmentUpdateModel> { attachmentModel });
                }
            }
        }
        private async Task AddAttributeAttachments(RequestAttribute attribute, List<RequestAttributeAttachmentUpdateModel> attachments)
        {
            foreach (var attachmentModel in attachments)
            {
                if (string.IsNullOrEmpty(attachmentModel.AttachmentAlt))
                {
                    throw new ArgumentException("AttachmentAlt is required");
                }

                attribute.RequestAttributeAttachments.Add(new RequestAttributeAttachment
                {
                    AttachmentUrl = attachmentModel.AttachmentUrl != null ? await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.REQUESTATTRIBUTE) : null,
                    AttachmentAlt = attachmentModel.AttachmentAlt
                });
            }
        }
        private async Task UpdateRequestAttachments(Request request, List<RequestAttachmentUpdateModel> attachments)
        {
            foreach (var attachmentModel in attachments)
            {
                var existingAttachment = request.RequestAttachments.FirstOrDefault(_ => _.Id == attachmentModel.Id);

                if (existingAttachment != null)
                {
                    existingAttachment.AttachmentUrl = attachmentModel.AttachmentUrl != null
                        ? await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.REQUEST)
                        : existingAttachment.AttachmentUrl;

                    existingAttachment.AttachmentAlt = attachmentModel.AttachmentAlt ?? existingAttachment.AttachmentAlt;
                }
                else
                {
                    await AddNewRequestAttachments(request, new List<RequestAttachmentUpdateModel> { attachmentModel });
                }
            }
        }
        private async Task AddNewRequestAttachments(Request request, List<RequestAttachmentUpdateModel> attachments)
        {
            foreach (var attachmentModel in attachments)
            {
                if (string.IsNullOrEmpty(attachmentModel.AttachmentAlt))
                {
                    throw new ArgumentException("AttachmentAlt is required");
                }

                request.RequestAttachments.Add(new RequestAttachment
                {
                    AttachmentUrl = attachmentModel.AttachmentUrl != null ? await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.REQUEST) : null,
                    AttachmentAlt = attachmentModel.AttachmentAlt
                });
            }
        }
        #endregion
        private async Task<string> UploadFile(IFormFile fileUrl, string folderName)
        {
            if (fileUrl == null)
            {
                throw new ArgumentException("File URL cannot be null or empty.");
            }

            return await _cloudinaryHelper.UploadImageAsync(
                fileUrl,
                folderName: folderName
            );
        }

       
    }
}
