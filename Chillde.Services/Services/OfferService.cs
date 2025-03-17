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
using Chillde.Repositories.Common;
using AutoMapper.Features;
using Chillde.Services.Models.FeatureModels;


namespace Chillde.Services.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ITranslationService _translationService;
        private readonly IStringLocalizer<OfferLanguage> _localizer;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public OfferService(IUnitOfWork unitOfWork, IClaimService claimService, ITranslationService translationService,
            IStringLocalizer<OfferLanguage> localizer, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _translationService = translationService;
            _localizer = localizer;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> GetAllAsync(OfferFilterModel filterParameter, Guid requestId,
            string sourceLanguageCode, string targetLanguageCode)
        {
            var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            try
            {
                var offersResult = await _unitOfWork.OfferRepository.GetAllAsync(
                    offer =>
                        offer.IsDeleted == filterParameter.IsDeleted &&
                        (!filterParameter.ItemId.HasValue ||
                         offer.Request.CategoryId == filterParameter.ItemId) &&
                        (!filterParameter.MinPrice.HasValue ||
                         offer.Service.Packages.Any(p => p.Price >= filterParameter.MinPrice)) &&
                        (!filterParameter.MaxPrice.HasValue ||
                         offer.Service.Packages.Any(p => p.Price <= filterParameter.MaxPrice)) &&
                        (!filterParameter.MinDeliveryTime.HasValue ||
                         offer.Service.Packages.Any(p => p.DeliveryTime >= filterParameter.MinDeliveryTime)) &&
                        (!filterParameter.MaxDeliveryTime.HasValue ||
                         offer.Service.Packages.Any(p => p.DeliveryTime <= filterParameter.MaxDeliveryTime)) &&
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
                List<OfferLocalierModel> localizedOffers;

                if (sourceLanguageCode != "en")
                {
                    var offersWithTranslations =
                        await _unitOfWork.OfferRepository.GetOffersWithTranslationsAsync(sourceLanguageCode, offerIds);
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
                        ServiceId = offer.ServiceId?? Guid.Empty,
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
            var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
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

                var result = new OfferLocalierModel
                {
                    Id = offer.Id,
                    Status = _localizer[offer.Status.ToString()],
                    Message = offer.Message,
                    RequestId = offer.RequestId,
                    ServiceId = offer.ServiceId,
                    CreatedById = offer.CreatedById,
                    CreationDate = offer.CreationDate
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

        public async Task<ResponseModel> AddAsync(OfferAddModel model, Guid requestId, string sourceLanguageCode,
            string targetLanguageCode)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "User is not authenticated."
                };
            }
            if (string.IsNullOrEmpty(model.Message) || !model.ServiceId.HasValue || model.ServiceId.Value == Guid.Empty)
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
                    CreatedById = currentUserId!.Value,
                    Message = model.Message,
                    MinWeight = model.MinWeight,
                    MaxWeight = model.MaxWeight,
                };
                if (model.OfferAttachmentAddModels != null)
                {
                    foreach (var attachmentModel in model.OfferAttachmentAddModels)
                    {
                        var offerAttachment = new OfferAttachment
                        {
                            AttachmentUrl = attachmentModel.AttachmentUrl != null ?  await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.OFFER) : null,
                            AttachmentAlt = attachmentModel.AttachmentAlt,
                        };
                        newOffer.OfferAttachments.Add(offerAttachment);
                    }
                }
                string? translatedMessage = null;

                if (!string.IsNullOrEmpty(sourceLanguageCode) && !string.IsNullOrEmpty(targetLanguageCode))
                {
                    var translationResponse =
                        await _translationService.TranslateAsync(model.Message, sourceLanguageCode, targetLanguageCode);

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
                var languageId =
                    (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode == "en"
                        ? sourceLanguageCode
                        : targetLanguageCode))!;
                if (!string.IsNullOrEmpty(translatedMessage))
                {
                    var translation = new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Offer",
                        EntityId = newOffer.Id,
                        FieldName = "Message",
                        TranslationText = targetLanguageCode == "en" ? model.Message : translatedMessage,
                        LanguageId = languageId
                    };
                    await _unitOfWork.TranslationRepository.AddAsync(translation);
                }

                if (model.FeatureAddModels != null)
                    foreach (var featureModel in model.FeatureAddModels)
                    {
                        var newFeature = new Feature
                        {
                            Id = Guid.NewGuid(),
                            Name = featureModel.Name,
                            Question = featureModel.Question,
                            QuestionType = featureModel.QuestionType,
                            IsInformationRequired = featureModel.IsInformationRequired,
                            IsQuantity = featureModel.IsQuantity,
                        };

                        await _unitOfWork.FeatureRepository.AddAsync(newFeature);

                        //if (featureModel.PackageFeatureAddModel != null)
                        //{
                        //    var packageFeatureModel = featureModel.PackageFeatureAddModel;
                        //    var newPackageFeature = new PackageFeature
                        //    {
                        //        Id = Guid.NewGuid(),
                        //        FeatureId = newFeature.Id,
                        //        Name = packageFeatureModel.Name,
                        //        IsExtra = packageFeatureModel.IsExtra,
                        //        AdditionalCost = packageFeatureModel.AdditionalCost,
                        //        AdditionalDay = packageFeatureModel.AdditionalDay,
                        //        IsChecked = packageFeatureModel.IsChecked,
                        //        MaxQuantity = packageFeatureModel.MaxQuantity
                        //    };

                        //    await _unitOfWork.PackageFeatureRepository.AddAsync(newPackageFeature);
                        //}

                        //var packageFeatures = new List<PackageFeature>();
                        //for (int i = 0; i < featureModel.PackageFeatureAddModels.Count; i++)
                        //{
                        //    var packageFeature = featureModel.PackageFeatureAddModels[i];
                        //    //string translatedQuestion = translationResponse.TranslatedFields[$"PackageFeature_{i}_Name"];
                        //    var newPackageFeature = new PackageFeature
                        //    {
                        //        //Name = sourceLanguageCode == "en" ? packageFeature.Name : translatedQuestion,
                        //        Name = packageFeature.Name,
                        //        AdditionalCost = packageFeature.AdditionalCost,
                        //        AdditionalDay = packageFeature.AdditionalDay,
                        //        IsExtra = packageFeature.IsExtra,
                        //        IsChecked = packageFeature.IsChecked,
                        //        MaxQuantity = packageFeature.MaxQuantity,
                        //        FeatureId = newFeature.Id,
                        //        //PackageId = package.Id
                        //    };
                        //    packageFeatures.Add(newPackageFeature);
                        //}
                        //await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);
                    }

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Offer created successfully with features."
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


        public async Task<ResponseModel> UpdateAsync(Guid offerId, OfferUpdateModel model, string sourceLanguageCode,
            string targetLanguageCode)
        {
            if (offerId == Guid.Empty)
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
                var existingOffer = await _unitOfWork.OfferRepository.GetAsync(offerId);
                if (existingOffer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer not found."
                    };
                }

                string? translatedMessage;
                bool offerMessageExists = existingOffer.Message == model.Message;
                var existingTranslation = await _unitOfWork.TranslationRepository
                    .GetTranslationAsync("Offer", offerId, "Message",
                        (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode))!);
                if (existingTranslation == null && targetLanguageCode == "en")
                {
                    existingTranslation = await _unitOfWork.TranslationRepository
                        .GetTranslationAsync("Offer", offerId, "Message",
                            (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(sourceLanguageCode))
                            !);
                }

                var translationToUpdate = await _unitOfWork.TranslationRepository
                    .GetTranslationAsync("Offer", offerId, "Message",
                        (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(sourceLanguageCode))!);
                bool translationExists =
                    existingTranslation != null && existingTranslation.TranslationText == model.Message;

                if (!offerMessageExists && !translationExists && model.Message != null)
                {
                    var translationResponse =
                        await _translationService.TranslateAsync(model.Message, sourceLanguageCode, targetLanguageCode);
                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate message."
                        };
                    }

                    translatedMessage = translationResponse.Message;
                    existingOffer.Message = targetLanguageCode == "en" ? translatedMessage : model.Message;
                    _unitOfWork.OfferRepository.Update(existingOffer);

                    if (!translationExists && existingTranslation != null)
                    {
                        if (targetLanguageCode != "en")
                        {
                            if (sourceLanguageCode == "en" && targetLanguageCode == "vi")
                            {
                                existingTranslation.TranslationText = translatedMessage;
                            }
                            else if (sourceLanguageCode == "vi" && targetLanguageCode == "en")
                            {
                                existingTranslation.TranslationText = model.Message;
                            }

                            _unitOfWork.TranslationRepository.Update(existingTranslation);
                        }
                        else
                        {
                            existingTranslation.TranslationText = model.Message;
                        }
                    }
                    else if (translationToUpdate != null)
                    {
                        translationToUpdate.TranslationText = model.Message;
                    }
                }

                if (existingOffer.Status != model.Status && model.Status != null)
                {
                    existingOffer.Status = (OfferStatus)model.Status;
                    if (model.Status == OfferStatus.Approved)
                    {
                        var existedOffers = await _unitOfWork.OfferRepository.GetAllAsync(
                            offer => offer.RequestId == existingOffer.RequestId && offer.Status != OfferStatus.Approved,
                            order: null, include: null, 1, 1000);
                        foreach (var offer in existedOffers.Data)
                        {
                            offer.Status = OfferStatus.Rejected;
                            _unitOfWork.OfferRepository.Update(offer);
                        }
                    }

                    _unitOfWork.OfferRepository.Update(existingOffer);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _unitOfWork.CommitTransactionAsync();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Offer updated successfully.",
                        Data = existingOffer
                    };
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status204NoContent,
                        Message = "No changes detected."
                    };
                }
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

                var translation = await _unitOfWork.TranslationRepository
                    .GetTranslationAsync("Offer", offerId, "Message",
                        (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync("vi"))!);
                if (translation != null)
                {
                    _unitOfWork.TranslationRepository.SoftRemove(translation);
                }

                _unitOfWork.OfferRepository.SoftRemove(existingOffer);
                var changes = await _unitOfWork.SaveChangeAsync();
                return changes > 0
                    ? new ResponseModel
                        { Code = StatusCodes.Status200OK, Message = "Offer and its translation deleted successfully." }
                    : new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to delete offer and translation."
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