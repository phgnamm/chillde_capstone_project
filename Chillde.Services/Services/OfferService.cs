using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OfferModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;



namespace Chillde.Services.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ITranslationService _translationService;
        public OfferService(IUnitOfWork unitOfWork, IClaimService claimService, ITranslationService translationService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _translationService = translationService;
        }

        public async Task<ResponseModel> GetAllAsync(OfferFilterModel filterParameter,Guid requestId, string sourceLanguageCode, string targetLanguageCode)
        {
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
                   include:null,
                   filterParameter.PageIndex,
                   filterParameter.PageSize
               );

                var offerIds = offersResult.Data.Select(offer => offer.Id).ToList();
                var offersWithTranslations = await _unitOfWork.OfferRepository.GetOffersWithTranslationsAsync(targetLanguageCode, offerIds);
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Offers retrieved successfully.",
                    Data = new
                    {
                        offersResult.TotalCount,
                        Results = offersWithTranslations
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

        public async Task<ResponseModel> AddAsync(OfferAddModel model,Guid requestId, string sourceLanguageCode, string targetLanguageCode)
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

            try
            {
                var newOffer = new Offer
                {
                    Status = OfferStatus.Pending,
                    RequestId = requestId,
                    ServiceId = model.ServiceId,
                    CreatedById = currentUserId.Value
                };

                if (sourceLanguageCode != "en")
                {
                    var translationResponse = await _translationService.TranslateAsync(model.Message, sourceLanguageCode, "en");

                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate message to English."
                        };
                    }
                    newOffer.Message = translationResponse.Message;
                }
                else
                {
                    newOffer.Message = model.Message;
                }

                await _unitOfWork.OfferRepository.AddAsync(newOffer);
                var changes = await _unitOfWork.SaveChangeAsync();

                if (sourceLanguageCode != "en" && !string.IsNullOrEmpty(model.Message))
                {
                    var translationResponseToTarget = await _translationService.TranslateAsync(model.Message, sourceLanguageCode, targetLanguageCode);

                    if (translationResponseToTarget.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate message to target language."
                        };
                    }
                    var translation = new Translation
                    {
                        EntityType = "Offer",
                        EntityId = newOffer.Id, 
                        FieldName = "Message",
                        TranslationText = translationResponseToTarget.Message,
                        LanguageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode)
                    };

                    await _unitOfWork.TranslationRepository.AddAsync(translation);
                    await _unitOfWork.SaveChangeAsync();
                }
                return changes > 0
                    ? new ResponseModel { Code = StatusCodes.Status201Created, Message = "Offer created successfully.", Data = newOffer }
                    : new ResponseModel { Code = StatusCodes.Status500InternalServerError, Message = "Failed to create offer." };
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
