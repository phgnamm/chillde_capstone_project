using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Services.Models.ResponseModels;
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
                await _unitOfWork.BeginTransactionAsync();
                var languageId = (Guid)await _unitOfWork.TranslationRepository.GetLanguageIdByCodeAsync(targetLanguageCode != "en" ? targetLanguageCode : sourceLanguageCode);
                var translationName = await _unitOfWork.TranslationRepository.GetTranslationAsync("Package", id, "Name", languageId);
                var translationDescription = await _unitOfWork.TranslationRepository.GetTranslationAsync("Package", id, "Description", languageId);
                bool changesMade = false;
                if (!string.Equals(packageUpdateModel.Name, package.Name, StringComparison.OrdinalIgnoreCase) &&
                  (translationName != null && !string.Equals(packageUpdateModel.Name, translationName.TranslationText, StringComparison.OrdinalIgnoreCase)))
                {
                    var translationResponse = await _translationService.TranslateAsync(packageUpdateModel.Name!, sourceLanguageCode, targetLanguageCode);
                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        throw new Exception("Failed to translate Name.");
                    }
                    translationName!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : packageUpdateModel.Name!;
                    package.Name = targetLanguageCode != "en" ? packageUpdateModel.Name : translationResponse.Message;
                    _unitOfWork.TranslationRepository.Update(translationName);
                    changesMade = true;
                }

                if (!string.Equals(packageUpdateModel.Description, package.Description, StringComparison.OrdinalIgnoreCase) &&
                  (translationDescription != null && !string.Equals(packageUpdateModel.Description, translationDescription.TranslationText, StringComparison.OrdinalIgnoreCase)))
                {
                    var translationResponse = await _translationService.TranslateAsync(packageUpdateModel.Description!, sourceLanguageCode, targetLanguageCode);
                    if (translationResponse.Code != StatusCodes.Status200OK)
                    {
                        throw new Exception("Failed to translate Description.");
                    }

                    translationDescription!.TranslationText = targetLanguageCode != "en" ? translationResponse.Message : packageUpdateModel.Description!;
                    package.Description = targetLanguageCode != "en" ? packageUpdateModel.Description : translationResponse.Message;
                    _unitOfWork.TranslationRepository.Update(translationDescription);
                    changesMade = true;
                }

                if (packageUpdateModel.Price != package.Price)
                {
                    package.Price = packageUpdateModel.Price;
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
                _unitOfWork.PackageRepository.Update(package);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

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

                Expression<Func<Repositories.Entities.Order, bool>> filter = order =>
                         order.PackageId == id &&
                         order.IsDeleted == false;

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                    filter: filter,
                    include: null
                );

                if (orders == null)
                {
                    _unitOfWork.PackageRepository.HardRemove(package);
                }

                _unitOfWork.PackageRepository.SoftRemove(package);
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

        public async Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, Guid packageId, string sourceLanguageCode, string targetLanguageCode)
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
                await _unitOfWork.BeginTransactionAsync();
                var fieldsToTranslate = new Dictionary<string, string>
                {
                    { "Name", featureAddModel.Name }
                    };
                for (int i = 0; i < featureAddModel.PackageFeatures.Count; i++)
                {
                    var packageFeature = featureAddModel.PackageFeatures[i];
                    fieldsToTranslate.Add($"PackageFeature_{i}_Name", packageFeature.Name);
                }

                var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                if (translationResponse.Code != StatusCodes.Status200OK)
                {
                    throw new Exception("Failed to translate fields.");
                }

                string translatedName = translationResponse.TranslatedFields["Name"];
                //var numberOfExistedPackage = _unitOfWork.PackageRepository.GetAllPackageFromService(packageId).Result.Count();
                //if (numberOfExistedPackage > 3)
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status422UnprocessableEntity,
                //        Message = "Number of packages cannot exceed 3."
                //    };
                //}

                var feature = new Feature
                {
                    Name = sourceLanguageCode == "en" ? featureAddModel.Name : translatedName,
                };

                await _unitOfWork.FeatureRepository.AddAsync(feature);

                var packageFeatures = new List<PackageFeature>();
                for (int i = 0; i < featureAddModel.PackageFeatures.Count; i++)
                {
                    var packageFeature = featureAddModel.PackageFeatures[i];
                    string translatedQuestion = translationResponse.TranslatedFields[$"PackageFeature_{i}_Name"];
                    var newPackageFeature = new PackageFeature
                    {
                        Name = sourceLanguageCode == "en" ? packageFeature.Name : translatedQuestion,
                        IsExtra = packageFeature.IsExtra,
                        AdditionalCost = packageFeature.AdditionalCost,
                        AdditionalDay = packageFeature.AdditionalDay,
                        FeatureId = feature.Id,
                        PackageId = packageId
                    };
                    packageFeatures.Add(newPackageFeature);
                }
                await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);

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
                if (!string.IsNullOrEmpty(featureAddModel.Name))
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        EntityType = "Feature",
                        EntityId = feature.Id,
                        FieldName = "Name",
                        TranslationText = sourceLanguageCode != "en" ? featureAddModel.Name : translatedName,
                        LanguageId = languageId.Value
                    });
                }

                var packageFeatureList = feature.PackageFeatures.ToList();

                foreach (var packageFeature in feature.PackageFeatures.Select((value, index) => new { value, index }))
                {
                    if (!string.IsNullOrEmpty(packageFeature.value.Name))
                    {
                        translations.Add(new Translation
                        {
                            Id = Guid.NewGuid(),
                            EntityType = "PackageFeature",
                            EntityId = packageFeatureList[packageFeature.index].Id,
                            FieldName = "Name",
                            TranslationText = packageFeature.value.Name,
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


        public async Task<ResponseModel> DeletePackageFeatureAsync(Guid packageId, Guid packageFeatureId)
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

                var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(packageFeatureId);
                if (packageFeature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package feature not found."
                    };
                }

                Expression<Func<Repositories.Entities.Order, bool>> filter = order =>
                         order.PackageId == packageId &&
                         order.IsDeleted == false;

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(
                    filter: filter,
                    include: null
                );

                if (orders == null)
                {
                    _unitOfWork.PackageFeatureRepository.HardRemove(packageFeature);
                }

                _unitOfWork.PackageFeatureRepository.SoftRemove(packageFeature);
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
    }
}
