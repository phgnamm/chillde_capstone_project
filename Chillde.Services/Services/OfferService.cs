using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System.Globalization;



namespace Chillde.Services.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ITranslationService _translationService;
        private readonly IStringLocalizer<OfferLanguage> _localizer;

        public OfferService(IUnitOfWork unitOfWork, IClaimService claimService, ITranslationService translationService, IStringLocalizer<OfferLanguage> localizer)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _translationService = translationService;
            _localizer = localizer;
        }

        public async Task<ResponseModel> GetAllAsync(OfferFilterModel filterParameter, Guid requestId, string sourceLanguageCode, string targetLanguageCode)
        {
            var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            try
            {
                var offersResult = await _unitOfWork.OfferRepository.GetAllAsync(
                    offer =>
                        offer.IsDeleted == filterParameter.IsDeleted &&
                        (!filterParameter.Status.HasValue || offer.Status == filterParameter.Status) &&
                        (!filterParameter.ServiceId.HasValue || offer.ServiceId == filterParameter.ServiceId) &&
                        (offer.RequestId == requestId) &&
                        (!filterParameter.CreatedById.HasValue || offer.CreatedById == filterParameter.CreatedById),
                    offers =>
                    {
                        switch (filterParameter.Order.ToLower())
                        {
                            case "status":
                                return filterParameter.OrderByDescending
                                    ? offers.OrderByDescending(offer => offer.Status)
                                    : offers.OrderBy(offer => offer.Status);
                            case "createdAt":
                                return filterParameter.OrderByDescending
                                    ? offers.OrderByDescending(offer => offer.CreationDate)
                                    : offers.OrderBy(offer => offer.CreationDate);
                            default:
                                return filterParameter.OrderByDescending
                                    ? offers.OrderByDescending(offer => offer.CreationDate)
                                    : offers.OrderBy(offer => offer.CreationDate);
                        }
                    },
                    include: null,
                    filterParameter.PageIndex,
                    filterParameter.PageSize
                );
                var offerIds = offersResult.Data.Select(offer => offer.Id).ToList();
                List<OfferLocalierModel> localizedOffers = new List<OfferLocalierModel>();
                
                if (sourceLanguageCode != "en")
                {
                    var offersWithTranslations = await _unitOfWork.OfferRepository.GetOffersWithTranslationsAsync(sourceLanguageCode, offerIds);
                    localizedOffers = offersWithTranslations.Select(offer => new OfferLocalierModel
                    {
                        Id = offer.Id,
                        Status = _localizer[offer.Status.ToString()],
                        Message = offer.Message,
                        RequestId = offer.RequestId,
                        ServiceId = offer.ServiceId,
                        CreatedById = offer.CreatedById,
                        CreationDate = offer.CreationDate
                    }).ToList();
                }
                else
                {
                    localizedOffers = offersResult.Data.Select(offer => new OfferLocalierModel
                    {
                        Id = offer.Id,
                        Status = _localizer[offer.Status.ToString()],
                        Message = offer.Message,
                        RequestId = offer.RequestId,
                        ServiceId = offer.ServiceId,
                        CreatedById = offer.CreatedById,
                        CreationDate = offer.CreationDate
                    }).ToList();
                }
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Offers retrieved successfully.",
                    Data = new
                    {
                        offersResult.TotalCount,
                        Results = localizedOffers
                    }
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

        public async Task<ResponseModel> GetByIdAsync(Guid id, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                var offer = await _unitOfWork.OfferRepository.GetOfferAsync(id, targetLanguageCode);

                if (offer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer not found."
                    };
                }

                var result = new
                {
                    offer.Id,
                    Status = _localizer[offer.Status.ToString()],
                    offer.Message,
                    offer.RequestId,
                    offer.ServiceId,
                    offer.CreatedById,
                    offer.CreationDate
                };

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Offer retrieved successfully.",
                    Data = result
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
      
        public async Task<ResponseModel> AddAsync(OfferAddModel model, Guid requestId, string sourceLanguageCode, string targetLanguageCode)
        {
            var currentUserId = _claimService.GetCurrentUserId;

            if (model == null || string.IsNullOrEmpty(model.Message) || model.ServiceId == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid data provided."
                };
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var newOffer = new Offer
                {
                    Status = OfferStatus.Pending,
                    RequestId = requestId,
                    ServiceId = model.ServiceId,
                    CreatedById = currentUserId.Value,
                    Message = model.Message
                };
                string? translatedMessage = null;

                if (!string.IsNullOrEmpty(sourceLanguageCode) && !string.IsNullOrEmpty(targetLanguageCode))
                {
                    var translationResponse = await _translationService.TranslateAsync(model.Message, sourceLanguageCode, targetLanguageCode);

                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate message."
                        };
                    }

                    translatedMessage = translationResponse.Message;
                    newOffer.Message = targetLanguageCode == "en" ? translatedMessage : model.Message;
                }
                await _unitOfWork.OfferRepository.AddAsync(newOffer);

                if (!string.IsNullOrEmpty(translatedMessage))
                {
                    var translation = new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Offer",
                        EntityId = newOffer.Id,
                        FieldName = "Message",
                        TranslationText = targetLanguageCode == "en" ? model.Message : translatedMessage,
                        LanguageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(
                            targetLanguageCode == "en" ? sourceLanguageCode : targetLanguageCode
                        )
                    };
                    await _unitOfWork.TranslationRepository.AddAsync(translation);
                }
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Offer created successfully.",
                    Data = newOffer
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



        public async Task<ResponseModel> UpdateAsync(Guid offerId, OfferUpdateModel model, string sourceLanguageCode, string targetLanguageCode)
        {
            if (offerId == Guid.Empty || model == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid data provided."
                };
            }

            try
            {
                var existingOffer = await _unitOfWork.OfferRepository.GetAsync(offerId);
                if (existingOffer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer not found."
                    };
                }

                existingOffer.Status = model.Status ?? existingOffer.Status;
                string originalMessage = existingOffer.Message;
                if (!string.IsNullOrEmpty(model.Message) && model.Message != existingOffer.Message)
                {
                    existingOffer.Message = model.Message;
                    if (sourceLanguageCode != "en")
                    {
                        var translationResponse = await _translationService.TranslateAsync(model.Message, sourceLanguageCode, "en");
                        if (translationResponse.Code == StatusCodes.Status200OK)
                        {
                            existingOffer.Message = translationResponse.Message;
                        }
                        else
                        {
                            return new ResponseModel
                            {
                                Code = StatusCodes.Status500InternalServerError,
                                Message = "Failed to translate message."
                            };
                        }
                    }

                    if (targetLanguageCode != "en")
                    {
                        var translationResponse = await _translationService.TranslateAsync(model.Message, sourceLanguageCode, targetLanguageCode);
                        if (translationResponse.Code == StatusCodes.Status200OK && !string.IsNullOrEmpty(translationResponse.Message))
                        {
                            var translation = new Translation
                            {
                                EntityType = "Offer",
                                EntityId = existingOffer.Id,
                                FieldName = "Message",
                                TranslationText = translationResponse.Message,
                                LanguageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode)
                            };

                            await _unitOfWork.TranslationRepository.AddAsync(translation);
                        }
                    }
                }
                _unitOfWork.OfferRepository.Update(existingOffer);
                var changes = await _unitOfWork.SaveChangeAsync();

                return changes > 0
                    ? new ResponseModel { Code = StatusCodes.Status200OK, Message = "Offer updated successfully.", Data = existingOffer }
                    : new ResponseModel { Code = StatusCodes.Status500InternalServerError, Message = "Failed to update offer." };
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


        public async Task<ResponseModel> DeleteAsync(Guid offerId)
        {
            if (offerId == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid offer ID."
                };
            }
            try
            {
                var existingOffer = await _unitOfWork.OfferRepository.GetAsync(offerId);
                if (existingOffer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer not found."
                    };
                }

                _unitOfWork.OfferRepository.SoftRemove(existingOffer);
                var changes = await _unitOfWork.SaveChangeAsync();

                return changes > 0
                    ? new ResponseModel { Code = StatusCodes.Status200OK, Message = "Offer deleted successfully." }
                    : new ResponseModel { Code = StatusCodes.Status500InternalServerError, Message = "Failed to delete offer." };
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


    }
}
