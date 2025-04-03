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
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.AccountModels;
using Microsoft.EntityFrameworkCore;
using Chillde.Services.Models.TranslationModels;


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
                         offer.Request!.CategoryId == filterParameter.ItemId) &&
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
                    include: o => o.Include(_ => _.CreatedBy).Include(_ => _.Service).Include(_ => _.Request),
                    filterParameter.PageIndex,
                    filterParameter.PageSize
                );
                var offerIds = offersResult.Data.Select(offer => offer.Id).ToList();
                List<OfferModel> localizedOffers;

                if (sourceLanguageCode != "en")
                {
                    var offersWithTranslations =
                        await _unitOfWork.OfferRepository.GetOffersWithTranslationsAsync(sourceLanguageCode, offerIds);
                    localizedOffers = offersWithTranslations.Select(offer => new OfferModel
                    {
                        Id = offer.Id,
                        Status = offer.Status != null ? _localizer[offer.Status.ToString()] : string.Empty,
                        Message = offer.Message,
                        MinWeight = offer.MinWeight,
                        MaxWeight = offer.MaxWeight,
                        OfferAttachments = offer.OfferAttachments.ToList(),
                        RequestId = offer.RequestId,
                        ServiceId = offer.ServiceId,
                        CreatedBy = new AccountLiteModel
                        {
                            Email = offer.CreatedBy.Email,
                            FirstName = offer.CreatedBy.FirstName,
                            LastName = offer.CreatedBy.LastName,
                            Image = offer.CreatedBy.Image
                        },
                        CreationDate = offer.CreationDate
                    }).ToList();
                }
                else
                {
                    localizedOffers = offersResult.Data.Select(offer => new OfferModel
                    {
                        Id = offer.Id,
                        Status = _localizer[offer.Status.ToString()],
                        Message = offer.Message,
                        MinWeight = offer.MinWeight,
                        MaxWeight = offer.MaxWeight,
                        OfferAttachments = offer.OfferAttachments.ToList(),
                        RequestId = offer.RequestId,
                        ServiceId = offer.ServiceId,
                        CreatedBy = new AccountLiteModel
                        {
                            Email = offer.CreatedBy.Email,
                            FirstName = offer.CreatedBy.FirstName,
                            LastName = offer.CreatedBy.LastName,
                            Image = offer.CreatedBy.Image
                        },
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

                var result = new OfferModel
                {
                    Id = offer.Id,
                    Status = offer.Status != null ? _localizer[offer.Status.ToString()] : string.Empty,
                    Message = offer.Message,
                    MinWeight = offer.MinWeight,
                    MaxWeight = offer.MaxWeight,
                    OfferAttachments = offer.OfferAttachments.ToList(),
                    RequestId = offer.RequestId,
                    ServiceId = offer.ServiceId,
                    CreatedBy = new AccountLiteModel
                    {
                        Email = offer.CreatedBy.Email,
                        FirstName = offer.CreatedBy.FirstName,
                        LastName = offer.CreatedBy.LastName,
                        Image = offer.CreatedBy.Image
                    },
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

        public async Task<ResponseModel> AddAsync(OfferAddModel model, Guid requestId, string sourceLanguageCode, string targetLanguageCode)
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

            if (string.IsNullOrEmpty(model.Message))
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
                var offerId = Guid.NewGuid();
                var newOffer = new Offer
                {
                    Id = offerId,
                    Status = OfferStatus.Pending,
                    RequestId = requestId,
                    ServiceId = model.ServiceId,
                    CreatedById = currentUserId.Value,
                    Message = model.Message,
                    MinWeight = model.MinWeight,
                    MaxWeight = model.MaxWeight,
                    Service = model.ServiceId.HasValue ? await _unitOfWork.ServiceRepository.GetAsync(model.ServiceId.Value) : null
                };

                await _unitOfWork.OfferRepository.AddAsync(newOffer);

                if (model.OfferAttachmentAddModels != null)
                {
                    foreach (var attachmentModel in model.OfferAttachmentAddModels)
                    {
                        if (!string.IsNullOrEmpty(attachmentModel.AttachmentUrl?.ToString()))
                        {
                            var offerAttachment = new OfferAttachment
                            {
                                AttachmentUrl = await UploadFile(attachmentModel.AttachmentUrl, FolderAttachment.OFFER),
                                AttachmentAlt = attachmentModel.AttachmentAlt
                            };
                            newOffer.OfferAttachments.Add(offerAttachment);
                        }
                    }
                }

                Dictionary<string, string> textsToTranslate = new Dictionary<string, string>
        {
            { "Message", model.Message }
        };

                Package? newPackage = null;
                List<Feature> features = new List<Feature>();
                List<PackageFeature> packageFeatures = new List<PackageFeature>();

                if (!model.ServiceId.HasValue && model.PackageAddModel != null)
                {
                    var packageId = Guid.NewGuid();
                    newPackage = new Package
                    {
                        Id = packageId,
                        OfferId = offerId,
                        Name = PackageName.Customized,
                        Description = model.PackageAddModel.Description,
                        Price = model.PackageAddModel.Price,
                        DeliveryTime = model.PackageAddModel.DeliveryTime,
                        SketchRevision = model.PackageAddModel.SketchRevision,
                    };

                    await _unitOfWork.PackageRepository.AddAsync(newPackage);
                    textsToTranslate.Add("Package.Description", model.PackageAddModel.Description);

                    if (model.FeatureAddModels != null)
                    {
                        foreach (var featureModel in model.FeatureAddModels)
                        {
                            var featureId = Guid.NewGuid();
                            var newFeature = new Feature
                            {
                                Id = featureId,
                                Name = featureModel.Name,
                                IsInformationRequired = featureModel.IsInformationRequired,
                                IsQuantity = featureModel.IsQuantity
                            };
                            features.Add(newFeature);

                            textsToTranslate.Add($"Feature.{featureId}.Name", featureModel.Name);

                            if (featureModel.PackageFeatureAddModels != null)
                            {
                                var packageFeatureId = Guid.NewGuid();
                                var newPackageFeature = new PackageFeature
                                {
                                    Id = packageFeatureId,
                                    FeatureId = featureId,
                                    PackageId = packageId,
                                    Name = featureModel.PackageFeatureAddModels.FirstOrDefault()!.Name,
                                    IsChecked = featureModel.PackageFeatureAddModels.FirstOrDefault()!.IsChecked,
                                };
                                packageFeatures.Add(newPackageFeature);

                                textsToTranslate.Add($"PackageFeature.{packageFeatureId}.Name",
                                    featureModel.PackageFeatureAddModels.FirstOrDefault()!.Name);
                            }
                        }
                    }
                }

                if (features.Any())
                    await _unitOfWork.FeatureRepository.AddRangeAsync(features);

                if (packageFeatures.Any())
                    await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);

                if (!string.IsNullOrEmpty(sourceLanguageCode) && !string.IsNullOrEmpty(targetLanguageCode))
                {
                    var translationResponse = await _translationService.TranslateMultipleFieldsAsync(
                        textsToTranslate, sourceLanguageCode, targetLanguageCode
                    );

                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status500InternalServerError,
                            Message = "Failed to translate fields."
                        };
                    }

                    if (translationResponse.TranslatedFields is Dictionary<string, string> translatedTexts)
                    {
                        foreach (var item in translatedTexts)
                        {
                            var entityType = item.Key.Split('.')[0];
                            var fieldName = item.Key.Split('.').Last();
                            var entityId = entityType switch
                            {
                                "Offer" => newOffer.Id,
                                "Package" => newPackage?.Id ?? Guid.Empty,
                                "Feature" => features.FirstOrDefault(f => $"Feature.{f.Id}.Name" == item.Key)?.Id ?? Guid.Empty,
                                "PackageFeature" => packageFeatures.FirstOrDefault(pf => $"PackageFeature.{pf.Id}.Name" == item.Key)?.Id ?? Guid.Empty,
                                _ => Guid.Empty
                            };

                            if (entityId != Guid.Empty)
                            {
                                await _translationService.SaveTranslationAsync(new TransaltionAddModel
                                {
                                    EntityType = entityType,
                                    EntityId = entityId,
                                    FieldName = fieldName,
                                    TranslationText = item.Value
                                }, "en");
                            }
                        }
                    }
                }

                //await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Offer created successfully."
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


        public async Task<ResponseModel> UpdateAsync(Guid offerId, OfferUpdateModel model)
        {
            if (offerId == Guid.Empty)
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
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Offer updated successfully.",
                        Data = existingOffer
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
                        (Guid)(await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync("en"))!);
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