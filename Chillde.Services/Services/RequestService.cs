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

                if (requestAddModel.AttachmentUrl != null)
                {
                    newRequest.AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                        requestAddModel.AttachmentUrl,
                        newRequest.Id.ToString(),
                        newRequest.Id.ToString());
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
            var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            try
            {
                if (sourceLanguageCode.ToLower() == "en")
                {
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
                    var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                        r => r.IsDeleted == filterParameter.IsDeleted
                    );

                    var requestIds = requests.Data.Select(r => r.Id).ToList();

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


        public async Task<ResponseModel> GetById(Guid id)
        {
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
            var existingRequestModel = new RequestGetByIdModel
            {
                Id = existingRequest.Id,
                Name = existingRequest.Name ?? "Unknown",
                Description = existingRequest.Description ?? "Unknown",
                MinBudget = (decimal)existingRequest.MinBudget!,
                MaxBudget = (decimal)existingRequest.MaxBudget!,
                Timeline = (int)existingRequest.Timeline!,
                AttachmentUrl = existingRequest.AttachmentUrl ?? "Unknown",
                Status = existingRequest.Status,
                ItemId = existingRequest.ItemId,
                ItemName = existingRequest.Item.Name ?? "Unknown",
                ItemCode = existingRequest.Item.Code ?? "Unknown",
                ItemImageUrl = existingRequest.Item.ImageUrl ?? "Unknown",
                RequestDetailGetByIdModels = existingRequest.RequestDetails.Select(_ => new RequestDetailGetByIdModel
                {
                    Id = _.Id,
                    Description = _.Description,
                    ItemAttributeId = _.AttributeId,
                    ItemAttributeName = _.Attribute.Name ?? "Unknown"
                }).ToList()
            };
            return new ResponseModel
            {
                Data = existingRequestModel,
                Message = "Get request detail success"
            };
        }

        public async Task<ResponseModel> Update(Guid id, RequestUpdateModel requestUpdateModel)
        {
            if (requestUpdateModel == null || id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input."
                };
            }
            var hasOffered = await _unitOfWork.OfferRepository.RequestHasOffered(id);
            if (hasOffered)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Request cannot update."
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

            var existingRequest = await _unitOfWork.RequestRepository.GetAsync(id, _ => _.Include(_ => _.RequestDetails));
            if (existingRequest == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Request not found."
                };
            }

            existingRequest.Name = requestUpdateModel.Name ?? existingRequest.Name;
            existingRequest.Description = requestUpdateModel.Description ?? existingRequest.Description;
            existingRequest.MinBudget = requestUpdateModel.MinBudget ?? existingRequest.MinBudget;
            existingRequest.MaxBudget = requestUpdateModel.MaxBudget ?? existingRequest.MaxBudget;
            existingRequest.Timeline = requestUpdateModel.Timeline ?? existingRequest.Timeline;
            existingRequest.ItemId = requestUpdateModel.ItemId != Guid.Empty ? requestUpdateModel.ItemId : existingRequest.ItemId;
            existingRequest.ModifiedById = currentUserId.Value;
            existingRequest.ModificationDate = DateTime.UtcNow;
            if (requestUpdateModel.AttachmentUrl != null)
            {
                existingRequest.AttachmentUrl = await _cloudinaryHelper.UploadImageAsync(
                    requestUpdateModel.AttachmentUrl,
                    existingRequest.Id.ToString(),
                    existingRequest.Id.ToString());
            }
            if (requestUpdateModel.RequestDetailUpdateModels != null)
            {
                foreach (var detail in requestUpdateModel.RequestDetailUpdateModels)
                {
                    var existingDetail = existingRequest.RequestDetails.FirstOrDefault(_ => _.Id == detail.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.Description = detail.Description ?? existingDetail.Description;
                        existingDetail.AttributeId = detail.AttributeId != Guid.Empty ? detail.AttributeId : existingDetail.AttributeId;
                        existingDetail.ModifiedById = currentUserId.Value;
                        existingDetail.ModificationDate = DateTime.UtcNow;
                    }
                    else
                    {
                        existingRequest.RequestDetails.Add(new RequestDetail
                        {
                            Id = Guid.NewGuid(),
                            Description = detail.Description,
                            AttributeId = detail.AttributeId,
                            CreatedById = currentUserId.Value,
                            CreationDate = DateTime.UtcNow
                        });
                    }
                }
            }

            _unitOfWork.RequestRepository.Update(existingRequest);
            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Request updated successfully."
                }
                : new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Failed to update request."
                };
        }
    }
}
