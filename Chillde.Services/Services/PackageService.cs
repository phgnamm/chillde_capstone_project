using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITranslationService _translationService;

        public PackageService(IUnitOfWork unitOfWork, IMapper mapper, ITranslationService translationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _translationService = translationService;
        }

        public async Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                var package = await _unitOfWork.PackageRepository.GetAsync(id);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }
                //await _unitOfWork.BeginTransactionAsync();
                //var languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode != "en" ? targetLanguageCode : sourceLanguageCode);
                //var translationName = await _unitOfWork.TranslationRepository.GetTranslationAsync("Package", id, "Name", languageId);
                //var translationDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync("Package", id, "Description", languageId);
                //bool changesMade = false;
                //if (!string.Equals(packageUpdateModel.Name, package.Name, StringComparison.OrdinalIgnoreCase) &&
                //  (translationName != null && !string.Equals(packageUpdateModel.Name, translationName.TranslationText, StringComparison.OrdinalIgnoreCase)))
                //{
                //    var translationResponse = await _translationService.TranslateAsync(packageUpdateModel.Name!, sourceLanguageCode, targetLanguageCode);
                //    if (translationResponse.Code != StatusCodes.Status200OK)
                //    {
                //        throw new Exception("Failed to translate Name.");
                //    }
                //    translationName!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : packageUpdateModel.Name!;
                //    package.Name = targetLanguageCode != "en" ? packageUpdateModel.Name : translationResponse.Message;
                //    _unitOfWork.TranslationRepository.Update(translationName);
                //    changesMade = true;
                //}

                //if (!string.Equals(packageUpdateModel.Description, package.Description, StringComparison.OrdinalIgnoreCase) &&
                //  (translationDescription != null && !string.Equals(packageUpdateModel.Description, translationDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
                //{
                //    var translationResponse = await _translationService.TranslateAsync(packageUpdateModel.Description!, sourceLanguageCode, targetLanguageCode);
                //    if (translationResponse.Code != StatusCodes.Status200OK)
                //    {
                //        throw new Exception("Failed to translate Description.");
                //    }

                //    translationDescription!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : packageUpdateModel.Description!;
                //    package.Description = targetLanguageCode != "en" ? packageUpdateModel.Description : translationResponse.Message;
                //    _unitOfWork.TranslationRepository.Update(translationDescription);
                //    changesMade = true;
                //}

                //if (packageUpdateModel.Price != package.Price)
                //{
                //    package.Price = packageUpdateModel.Price;
                //    changesMade = true;
                //}

                //if (!changesMade)
                //{
                //    await _unitOfWork.RollbackTransactionAsync();
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status204NoContent,
                //        Message = "No changes detected."
                //    };
                //}

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(id);

                if (!anyOrder.Result)
                {
                    _mapper.Map(packageUpdateModel, package);
                    _unitOfWork.PackageRepository.Update(package);
                }
                else
                {
                    _unitOfWork.PackageRepository.SoftRemove(package);
                    Package newPackage = _mapper.Map<Package>(packageUpdateModel);
                    newPackage.ServiceId = package.ServiceId;
                    await _unitOfWork.PackageRepository.AddAsync(newPackage);
                }

                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Package updated successfully.",
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

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            try
            {
                var package = await _unitOfWork.PackageRepository.GetAsync(id);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(id);

                if (!anyOrder.Result)
                {
                    _unitOfWork.PackageRepository.HardRemove(package);
                }
                else
                {
                    _unitOfWork.PackageRepository.SoftRemove(package);
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

        public async Task<ResponseModel> GetAllFeatureAsync(FeatureFilterModel model, Guid packageId, string sourceLanguageCode, string targetLanguage)
        {
            try
            {
                var culture = sourceLanguageCode.ToLower() == "vi" ? "vi-VN" : "en-US";
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

                var package = await _unitOfWork.PackageRepository.GetAsync(packageId);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }
                Expression<Func<Feature, bool>> filter = feature =>
                    feature.IsDeleted == model.IsDeleted &&
                    feature.PackageFeatures.Any(_ => _.PackageId == packageId);

                Func<IQueryable<Feature>, IQueryable<Feature>> include = features =>
                    features.Include(f => f.PackageFeatures);

                var features = await _unitOfWork.FeatureRepository.GetAllAsync(
                    filter: filter,
                    include: include,
                    pageIndex: model.PageIndex,
                    pageSize: model.PageSize
                );

                if (!features.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "No features found for the specified package."
                    };
                }

                if (sourceLanguageCode.ToLower() == "en")
                {
                    var featureModels = _mapper.Map<List<FeatureModel>>(features.Data);
                    var result = new Pagination<FeatureModel>(featureModels, model.PageIndex, model.PageSize, features.TotalCount);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully retrieved features.",
                        Data = result
                    };
                }
                else 
                {
                    var featureIds = features.Data.Select(f => f.Id).ToList();
                    var translationFields = new[] { "Name" };

                    var translations = await _unitOfWork.TranslationRepository.GetEntitiesWithTranslationsAsync<Feature, FeatureModel>(
                        featureIds,
                        sourceLanguageCode,
                        feature => new FeatureModel
                        {
                            Name = feature.Name,
                            PackageFeatures = feature.PackageFeatures.Select(pf => new PackageFeature
                            {
                                Id = pf.Id,
                                Name = pf.Name,
                                IsExtra = pf.IsExtra,
                                AdditionalCost = pf.AdditionalCost,
                                AdditionalDay = pf.AdditionalDay
                            }).ToList()
                        },
                        "PackageFeatures",
                        translationFields,
                        null!,
                        "Question"
                    );

                    if (translations != null)
                    {
                        var result = new Pagination<FeatureModel>(translations.ToList(), model.PageIndex, model.PageSize, features.TotalCount);
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status200OK,
                            Message = "Successfully retrieved features with translations.",
                            Data = result
                        };
                    }

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status500InternalServerError,
                        Message = "Failed to retrieve translations."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred while retrieving features: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> AddPackageFeatureAsync(PackageFeatureAddModel packageFeatureAddModel, Guid packageId, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                var package = await _unitOfWork.PackageRepository.GetAsync(packageId);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                var feature = await _unitOfWork.FeatureRepository.GetAsync(packageFeatureAddModel.FeatureId);
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                //await _unitOfWork.BeginTransactionAsync();
                //var fieldsToTranslate = new Dictionary<string, string>
                //{
                //    { "Name", packageFeatureAddModel.Name }
                //    };

                //var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                //if (translationResponse.Code != StatusCodes.Status200OK)
                //{
                //    throw new Exception("Failed to translate fields.");
                //}

                //string translatedName = translationResponse.TranslatedFields["Name"];

                Expression<Func<PackageFeature, bool>> filter = _ =>
                _.FeatureId == packageFeatureAddModel.FeatureId &&
                   _.IsDeleted == false;

                var numberOfExistedPackageFeature = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                    filter: filter,
                    include: null
                );

                //var numberOfExistedPackageFeature = _unitOfWork.PackageRepository.GetAllPackageFromService(packageId).Result.Count();
                var maxPackageFeature = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumFeatureOfOnePackage).Result;
                if (numberOfExistedPackageFeature.TotalCount > int.Parse(maxPackageFeature!))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = $"Number of package cannot exceed {maxPackageFeature}."
                    };
                }

                var newPackageFeature = new PackageFeature
                {
                    //Name = sourceLanguageCode == "en" ? packageFeatureAddModel.Name : translatedName,
                    Name = packageFeatureAddModel.Name,
                    IsExtra = packageFeatureAddModel.IsExtra,
                    AdditionalCost = packageFeatureAddModel.AdditionalCost,
                    AdditionalDay = packageFeatureAddModel.AdditionalDay,
                    MaxQuantity = packageFeatureAddModel.MaxQuantity,
                    IsChecked = packageFeatureAddModel.IsChecked,
                    FeatureId = feature.Id,
                    PackageId = packageId
                };

                await _unitOfWork.PackageFeatureRepository.AddAsync(newPackageFeature);

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
                //if (!string.IsNullOrEmpty(packageFeatureAddModel.Name))
                //{
                //    translations.Add(new Translation
                //    {
                //        Id = Guid.NewGuid(),
                //        EntityType = "Feature",
                //        EntityId = feature.Id,
                //        FieldName = "Name",
                //        TranslationText = sourceLanguageCode != "en" ? packageFeatureAddModel.Name : translatedName,
                //        LanguageId = languageId.Value
                //    });
                //}

                //var packageFeatureList = feature.PackageFeatures.ToList();

                //foreach (var packageFeature in feature.PackageFeatures.Select((value, index) => new { value, index }))
                //{
                //    if (!string.IsNullOrEmpty(packageFeature.value.Name))
                //    {
                //        translations.Add(new Translation
                //        {
                //            Id = Guid.NewGuid(),
                //            EntityType = "PackageFeature",
                //            EntityId = packageFeatureList[packageFeature.index].Id,
                //            FieldName = "Name",
                //            TranslationText = packageFeature.value.Name,
                //            LanguageId = languageId.Value
                //        });
                //    }
                //}
                //await _unitOfWork.TranslationRepository.AddRangeAsync(translations);
                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created."
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

    }
}
