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
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using OpenAI.GPT3.Interfaces;
using Chillde.Services.Helpers;
using Chillde.Services.Utils;
using CloudinaryDotNet;
using Chillde.Services.Common;
using System.Linq;
using Chillde.Repositories.Models.RequestModels;
using Chillde.Repositories.Models.ShippingAddressModels;
using AutoMapper;


namespace Chillde.Services.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ITranslationService _translationService;
        private readonly IStringLocalizer<OfferLanguage> _localizer;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IRedisHelper _redisHelper;
        private readonly IMapper _mapper;

        public OfferService(IUnitOfWork unitOfWork, IClaimService claimService, ITranslationService translationService,
            IStringLocalizer<OfferLanguage> localizer, ICloudinaryHelper cloudinaryHelper, IRedisHelper redisHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _translationService = translationService;
            _localizer = localizer;
            _cloudinaryHelper = cloudinaryHelper;
            _redisHelper = redisHelper;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetAllAsync(OfferFilterModel filterParameter,
            string sourceLanguageCode, string targetLanguageCode)
        {
            var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            try
            {
                var currentUserId = _claimService.GetCurrentUserId!.Value;
                var cacheKey = $"offers_{sourceLanguageCode}_{targetLanguageCode}_{CacheTools.GenerateCacheKey(filterParameter)}";
                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    var offersResult = await _unitOfWork.OfferRepository.GetAllAsync(
                    offer =>
                        offer.IsDeleted == filterParameter.IsDeleted &&
                        (!filterParameter.CategoryId.HasValue ||
                        offer.Request!.CategoryId == filterParameter.CategoryId) &&
                        (!filterParameter.MinPrice.HasValue ||
                        (offer.Package != null && offer.Package.Price >= filterParameter.MinPrice)) &&
                        (!filterParameter.MaxPrice.HasValue ||
                        (offer.Package != null && offer.Package.Price <= filterParameter.MaxPrice)) &&
                        (!filterParameter.MinDeliveryTime.HasValue ||
                        (offer.Package != null && offer.Package.DeliveryTime >= filterParameter.MinDeliveryTime)) &&
                        (!filterParameter.MaxDeliveryTime.HasValue ||
                        (offer.Package != null && offer.Package.DeliveryTime <= filterParameter.MaxDeliveryTime)) &&
                        (!filterParameter.Status.HasValue || offer.Status == filterParameter.Status) &&
                        (!filterParameter.ServiceId.HasValue || offer.ServiceId == filterParameter.ServiceId) &&
                        (!filterParameter.RequestId.HasValue || offer.RequestId == filterParameter.RequestId) &&
                        filterParameter.ViewAll || offer.CreatedById == currentUserId,
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
                    include: o => o.Include(_ => _.CreatedBy).ThenInclude(_ => _.ShippingAddresses).Include(_ => _.Service).Include(_ => _.Request).Include(_ => _.Package).ThenInclude(_ => _.PackageFeatures).ThenInclude(_ => _.Feature).Include(_ => _.OfferAttachments),
                    filterParameter.PageIndex,
                    filterParameter.PageSize
                );
                    var offerIds = offersResult.Data.Select(offer => offer.Id).ToList();
                    List<OfferModel> localizedOffers;

                    if (sourceLanguageCode != "vi")
                    {
                        var offersWithTranslations =
                            await _unitOfWork.OfferRepository.GetOffersWithTranslationsAsync(sourceLanguageCode, offerIds);
                        localizedOffers = offersWithTranslations.Select(offer => new OfferModel
                        {
                            Id = offer.Id,
                            IsDeleted = offer.IsDeleted,
                            Status = offer.Status,
                            Message = offer.Message,
                            MinWeight = offer.MinWeight,
                            MaxWeight = offer.MaxWeight,
                            OfferAttachments = offer.OfferAttachments?.ToList(),
                            RequestId = offer.RequestId,
                            ServiceId = offer.ServiceId,
                            ShippingAddress = _mapper.Map<ShippingAddressModel?>(offer.CreatedBy.ShippingAddresses.FirstOrDefault()),
                            Package = new PackageModel
                            {
                                Id = offer.Package!.Id,
                                Name = offer.Package!.Name,
                                Description = offer.Package!.Description,
                                Price = offer.Package!.Price,
                                DeliveryTime = offer.Package!.DeliveryTime,
                                MaxQuantity = offer.Package!.MaxQuantity,
                                SketchRevision = offer.Package!.SketchRevision,
                                ResponseTime = TimeSpan.FromMinutes(offer.Package!.ResponseTime),
                                Features = offer.Package.PackageFeatures?
                .Select(pf => pf.Feature)
                .Distinct()
                .Select(feature => new FeatureModel
                {
                    Id = feature.Id,
                    Name = feature.Name,
                    Question = feature.Question,
                    QuestionType = feature.QuestionType,
                    IsInformationRequired = feature.IsInformationRequired,
                    IsQuantity = feature.IsQuantity,
                    PackageFeatures = offer.Package.PackageFeatures
                        .Where(pf => pf.FeatureId == feature.Id)
                        .Select(pf => new PackageFeature
                        {
                            Id = pf.Id,
                            Name = pf.Name,
                            AdditionalCost = pf.AdditionalCost,
                            AdditionalDay = pf.AdditionalDay,
                            IsExtra = pf.IsExtra,
                            IsChecked = pf.IsChecked,
                            MaxQuantity = pf.MaxQuantity,
                        }).ToList()
                }).ToList()
                            },
                            CreatedBy = new AccountLiteModel
                            {
                                Email = offer.CreatedBy.Email,
                                FirstName = offer.CreatedBy.FirstName,
                                LastName = offer.CreatedBy.LastName,
                                Username = offer.CreatedBy.Username,
                                Image = offer.CreatedBy.Image,
                            },
                            CreationDate = offer.CreationDate
                        }).ToList();
                    }

                    else
                    {
                        localizedOffers = offersResult.Data.Select(offer => new OfferModel
                        {
                            Id = offer.Id,
                            IsDeleted = offer.IsDeleted,
                            Status = offer.Status,
                            Message = offer.Message,
                            MinWeight = offer.MinWeight,
                            MaxWeight = offer.MaxWeight,
                            OfferAttachments = offer.OfferAttachments?.ToList(),
                            RequestId = offer.RequestId,
                            ServiceId = offer.ServiceId,
                            ShippingAddress = _mapper.Map<ShippingAddressModel?>(offer.CreatedBy.ShippingAddresses.FirstOrDefault()),
                            Package = new PackageModel
                            {
                                Id = offer.Package!.Id,
                                Name = offer.Package!.Name,
                                Description = offer.Package!.Description,
                                Price = offer.Package!.Price,
                                DeliveryTime = offer.Package!.DeliveryTime,
                                MaxQuantity = offer.Package!.MaxQuantity,
                                SketchRevision = offer.Package!.SketchRevision,
                                ResponseTime = TimeSpan.FromMinutes(offer.Package!.ResponseTime),
                                Features = offer.Package.PackageFeatures?
                .Select(pf => pf.Feature)
                .Distinct()
                .Select(feature => new FeatureModel
                {
                    Id = feature.Id,
                    Name = feature.Name,
                    Question = feature.Question,
                    QuestionType = feature.QuestionType,
                    IsInformationRequired = feature.IsInformationRequired,
                    IsQuantity = feature.IsQuantity,
                    PackageFeatures = offer.Package.PackageFeatures
                        .Where(pf => pf.FeatureId == feature.Id)
                        .Select(pf => new PackageFeature
                        {
                            Id = pf.Id,
                            Name = pf.Name,
                            AdditionalCost = pf.AdditionalCost,
                            AdditionalDay = pf.AdditionalDay,
                            IsExtra = pf.IsExtra,
                            IsChecked = pf.IsChecked,
                            MaxQuantity = pf.MaxQuantity,
                        }).ToList()
                }).ToList()
                            },
                            CreatedBy = new AccountLiteModel
                            {
                                Email = offer.CreatedBy.Email,
                                FirstName = offer.CreatedBy.FirstName,
                                LastName = offer.CreatedBy.LastName,
                                Username = offer.CreatedBy.Username,
                                Image = offer.CreatedBy.Image
                            },
                            CreationDate = offer.CreationDate
                        }).ToList();
                    }
                    var result = new Pagination<OfferModel>
                    (
                       localizedOffers,
                       filterParameter.PageIndex,
                       filterParameter.PageSize,
                       localizedOffers.Count
                    );
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Offers retrieved successfully.",
                        Data = result
                    };
            });
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
                var currentUserId = _claimService.GetCurrentUserId!.Value;
                var cacheKey = $"offers_{id}";
                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    var offer = await _unitOfWork.OfferRepository.GetOfferAsync(id, sourceLanguageCode);

                    if (offer == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "Offer not found."
                        };
                    }

                    var offerModel = new OfferModel
                    {
                        Id = offer.Id,
                        Status = offer.Status,
                        Message = offer.Message,
                        MinWeight = offer.MinWeight,
                        MaxWeight = offer.MaxWeight,
                        OfferAttachments = offer.OfferAttachments?.ToList(),
                        RequestId = offer.RequestId,
                        ServiceId = offer.ServiceId,
                        ShippingAddress = _mapper.Map<ShippingAddressModel?>(offer.CreatedBy.ShippingAddresses.FirstOrDefault()),
                        CreatedBy = new AccountLiteModel
                        {
                            Email = offer.CreatedBy.Email,
                            FirstName = offer.CreatedBy.FirstName,
                            LastName = offer.CreatedBy.LastName,
                            Image = offer.CreatedBy.Image
                        },
                        CreationDate = offer.CreationDate,
                        Package = offer.Package == null ? null : new PackageModel
                        {
                            Name = offer.Package.Name,
                            Id = offer.Package.Id,
                            Description = offer.Package.Description,
                            Price = offer.Package.Price,
                            DeliveryTime = offer.Package.DeliveryTime,
                            MaxQuantity = offer.Package.MaxQuantity,
                            SketchRevision = offer.Package.SketchRevision,
                            ResponseTime = TimeSpan.FromMinutes(offer.Package!.ResponseTime),
                            Features = offer.Package.PackageFeatures?
                                .Select(pf => pf.Feature)
                                .Distinct()
                                .Select(feature => new FeatureModel
                                {
                                    Id = feature.Id,
                                    Name = feature.Name,
                                    Question = feature.Question,
                                    QuestionType = feature.QuestionType,
                                    IsInformationRequired = feature.IsInformationRequired,
                                    IsQuantity = feature.IsQuantity,
                                    PackageFeatures = offer.Package.PackageFeatures
                                        .Where(pf => pf.FeatureId == feature.Id)
                                        .Select(pf => new PackageFeature
                                        {
                                            Id = pf.Id,
                                            Name = pf.Name,
                                            AdditionalCost = pf.AdditionalCost,
                                            AdditionalDay = pf.AdditionalDay,
                                            IsExtra = pf.IsExtra,
                                            IsChecked = pf.IsChecked,
                                            MaxQuantity = pf.MaxQuantity,
                                        }).ToList()
                                }).ToList()
                        }
                    };

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Offer retrieved successfully.",
                        Data = offerModel
                    };
                });
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

        public async Task<ResponseModel> AddAsync(OfferAddModel model, string sourceLanguageCode, string targetLanguageCode)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (currentUserId.HasValue && model.RequestId.HasValue)
            {
                var checkIsAccountOffer = await _unitOfWork.RequestRepository
                    .HasUserOfferedForRequestAsync(currentUserId.Value, model.RequestId.Value);

                if (checkIsAccountOffer)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "You already created an offer for this request"
                    };
                }
            }

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
                    RequestId = model.RequestId,
                    CreatedById = currentUserId.Value,
                    Message = model.Message,
                    MinWeight = model.MinWeight,
                    MaxWeight = model.MaxWeight,
                };

                await _unitOfWork.OfferRepository.AddAsync(newOffer);

                for (int i = 0; i < model.OfferAttachmentAddModels!.Count; i++)
                {
                    Models.OfferAttachmentModels.OfferAttachmentAddModel? attachment = model.OfferAttachmentAddModels[i];
                    if (attachment.AttachmentUrl == null) continue;

                    var id = Guid.NewGuid();

                    var uploadedUrl = await _cloudinaryHelper.UploadImageAsync(
                        attachment.AttachmentUrl,
                        attachment.AttachmentAlt,
                        id.ToString(),
                        folderName: FolderAttachment.OFFER
                    );

                    newOffer.OfferAttachments.Add(new OfferAttachment
                    {
                        Id = id,
                        AttachmentAlt = attachment.AttachmentAlt,
                        AttachmentUrl = uploadedUrl,
                        OfferId = newOffer.Id
                    });
                }

                Dictionary<string, string> textsToTranslate = new Dictionary<string, string>
        {
            { "Offer.Message", model.Message }
        };

                Package? newPackage = null;
                List<Feature> features = new List<Feature>();
                List<PackageFeature> packageFeatures = new List<PackageFeature>();

                var packageId = Guid.NewGuid();
                newPackage = new Package
                {
                    Id = packageId,
                    OfferId = offerId,
                    Name = PackageName.Customized,
                    Description = model.PackageAddModel!.Description,
                    Price = model.PackageAddModel.Price,
                    DeliveryTime = model.PackageAddModel.DeliveryTime,
                    ResponseTime = (float)model.PackageAddModel.ResponseTime.TotalMinutes,
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

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Offer created successfully.",
                    Data = newOffer.Id
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
                var existingOffer = await _unitOfWork.OfferRepository.GetOfferAsync(offerId, sourceLanguageCode);
                if (existingOffer == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer not found."
                    };
                }
                if (!existingOffer.Status.Equals(OfferStatus.Pending))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Offer can not update."
                    };
                }

                // Update basic fields
                var textsToTranslate = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(model.Message) && model.Message != existingOffer.Message)
                {
                    existingOffer.Message = model.Message;
                    textsToTranslate.Add("Offer.Message", model.Message);
                }
                existingOffer.ServiceId = model.ServiceId ?? existingOffer.ServiceId;
                existingOffer.MinWeight = model.MinWeight ?? existingOffer.MinWeight;
                existingOffer.MaxWeight = model.MaxWeight ?? existingOffer.MaxWeight;

                // Handle status change
                if (model.Status.HasValue && existingOffer.Status != model.Status)
                {
                    existingOffer.Status = model.Status.Value;

                    if (existingOffer.Status == OfferStatus.Approved)
                    {
                        var existedOffers = await _unitOfWork.OfferRepository.GetAllAsync(
                            offer => offer.RequestId == existingOffer.RequestId && offer.Status != OfferStatus.Approved,
                            order: null, include: null, pageIndex: 1, pageSize: 1000);

                        foreach (var offer in existedOffers.Data)
                            offer.Status = OfferStatus.Rejected;

                        _unitOfWork.OfferRepository.UpdateRange(existedOffers.Data);
                    }
                }

                // Update Package
                var existingPkg = existingOffer.Package;
                if (model.PackageUpdateModel != null && existingOffer.Package != null)
                {
                    var pkg = model.PackageUpdateModel;
                    if (!string.IsNullOrWhiteSpace(pkg.Description) && pkg.Description != existingPkg.Description)
                    {
                        existingPkg.Description = pkg.Description;
                        textsToTranslate.Add("Package.Description", pkg.Description);
                    }
                    existingPkg!.Price = pkg.Price != 0 ? pkg.Price : existingPkg?.Price;
                    existingPkg!.DeliveryTime = pkg.DeliveryTime ?? existingPkg.DeliveryTime;
                    existingPkg.SketchRevision = pkg.SketchRevision ?? existingPkg.SketchRevision;
                    if (pkg.MinQuantity != 0)
                    {
                        existingPkg.MinQuantity = pkg.MinQuantity;
                    }
                    existingPkg.MaxQuantity = pkg.MaxQuantity ?? existingPkg.MaxQuantity;
                    existingPkg.ResponseTime = (float)pkg.ResponseTime.TotalMinutes;
                }

                // add new OfferAttachment
                if (model.OfferAttachmentAddModels?.Any() == true)
                {
                    var newAttachments = new List<OfferAttachment>();

                    foreach (var attachment in model.OfferAttachmentAddModels)
                    {
                        if (attachment.AttachmentUrl == null) continue;

                        var id = Guid.NewGuid();

                        var uploadedUrl = await _cloudinaryHelper.UploadImageAsync(
                            attachment.AttachmentUrl,
                            attachment.AttachmentAlt,
                            id.ToString(),
                            folderName: FolderAttachment.OFFER
                        );

                        newAttachments.Add(new OfferAttachment
                        {
                            Id = id,
                            AttachmentAlt = attachment.AttachmentAlt,
                            AttachmentUrl = uploadedUrl,
                            OfferId = existingOffer.Id
                        });
                    }

                    if (newAttachments.Any())
                    {
                        await _unitOfWork.OfferAttachmentRepository.AddRangeAsync(newAttachments);
                    }
                }

                // delete OfferAttachment by ID
                if (model.OfferAttachmentIdsDeleting?.Any() == true)
                {
                    var offerAttachmentsToDelete = await _unitOfWork.OfferAttachmentRepository.GetAllAsync(
                        filter: x => model.OfferAttachmentIdsDeleting.Contains(x.Id)
                    );

                    if (offerAttachmentsToDelete == null || !offerAttachmentsToDelete.Data.Any())
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "Attachments not found."
                        };
                    }

                    var publicIdsToDelete = offerAttachmentsToDelete.Data.Select(x => x.Id.ToString()).ToList();
                    await _cloudinaryHelper.RemoveImagesAsync(publicIdsToDelete);
                    _unitOfWork.OfferAttachmentRepository.HardRemoveRange(offerAttachmentsToDelete.Data);
                }
                var existingPackageFeatures = existingOffer.Package!.PackageFeatures!.ToList();

                var packageFeaturesToUpdate = new List<PackageFeature>();
                var packageFeaturesToAdd = new List<PackageFeature>();

                foreach (var featureModel in model.featureUpdateModels!)
                {
                    foreach (var pfModel in featureModel.PackageFeatureAddModels)
                    {
                        var existing = existingPackageFeatures
                            .FirstOrDefault(x => x.FeatureId == featureModel.FeatureId);

                        if (existing != null)
                        {
                            if (!string.IsNullOrWhiteSpace(pfModel.Name) && pfModel.Name != existing.Name)
                            {
                                existing.Name = pfModel.Name;
                                textsToTranslate.Add($"PackageFeature.{existing.FeatureId}.Name", pfModel.Name);
                            }
                            existing.AdditionalCost = pfModel.AdditionalCost;
                            existing.AdditionalDay = pfModel.AdditionalDay;
                            existing.IsExtra = pfModel.IsExtra;
                            existing.IsChecked = pfModel.IsChecked;
                            existing.MaxQuantity = pfModel.MaxQuantity;

                            packageFeaturesToUpdate.Add(existing);
                        }
                        else
                        {
                            var newPackageFeatureId = new Guid();
                            var newPackageFeature = new PackageFeature
                            {
                                Id = newPackageFeatureId,
                                FeatureId = featureModel.FeatureId!.Value,
                                Name = pfModel.Name,
                                AdditionalCost = pfModel.AdditionalCost,
                                AdditionalDay = pfModel.AdditionalDay,
                                IsExtra = pfModel.IsExtra,
                                IsChecked = pfModel.IsChecked,
                                MaxQuantity = pfModel.MaxQuantity,
                                PackageId = existingOffer.Package.Id
                            };
                            textsToTranslate.Add($"PackageFeature.{featureModel.FeatureId!.Value}.Name", pfModel.Name);
                            packageFeaturesToAdd.Add(newPackageFeature);
                        }
                    }
                }
                if (packageFeaturesToUpdate.Any())
                    _unitOfWork.PackageFeatureRepository.UpdateRange(packageFeaturesToUpdate);

                if (packageFeaturesToAdd.Any())
                    await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeaturesToAdd);

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
                                "Offer" => offerId,
                                "Package" => existingPkg?.Id ?? Guid.Empty,
                                "PackageFeature" => packageFeaturesToAdd
                                                    .Concat(packageFeaturesToUpdate)
                                                    .FirstOrDefault(pf => $"PackageFeature.{pf.FeatureId}.Name" == item.Key)?.FeatureId ?? Guid.Empty,
                                _ => Guid.Empty
                            };

                            if (entityId != Guid.Empty)
                            {
                                await _translationService.UpdateTranslationAsync(new TransaltionAddModel
                                {
                                    EntityType = entityType,
                                    EntityId = entityId,
                                    FieldName = fieldName,
                                    TranslationText = item.Value
                                }, "en");
                            }
                        }
                    }
                    _unitOfWork.OfferRepository.Update(existingOffer);

                    var changes = await _unitOfWork.SaveChangeAsync();
                    if (changes > 0)
                    {
                        await _redisHelper.InvalidateCacheByPatternAsync($"offer_{offerId}");
                        await _redisHelper.InvalidateCacheByPatternAsync("offers_*");
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
                return new ResponseModel
                {
                    Code = StatusCodes.Status204NoContent,
                    Message = "No changes detected."
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


        public async Task<ResponseModel> UpdateStatusAsync(Guid offerId, OfferStatus status)
        {
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
                existingOffer.Status = status;
                if (existingOffer.Status == OfferStatus.Approved)
                {
                    var existedOffers = await _unitOfWork.OfferRepository.GetAllAsync(
                        offer => offer.RequestId == existingOffer.RequestId && offer.Status != OfferStatus.Approved,
                        order: null, include: null, pageIndex: 1, pageSize: 1000);

                    foreach (var offer in existedOffers.Data)
                        offer.Status = OfferStatus.Rejected;

                    _unitOfWork.OfferRepository.UpdateRange(existedOffers.Data);
                }
                await _unitOfWork.SaveChangeAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Offer status updated successfully."
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
    }
}