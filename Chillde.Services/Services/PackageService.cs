using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Repositories.Models.PackageModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Repositories.Models.ServiceWishlistModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Chillde.Services.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Globalization;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITranslationService _translationService;
        private readonly IBadWordFilterService _badWordFilterService;
        private readonly IRedisHelper _redisHelper;

        public PackageService(IUnitOfWork unitOfWork,
            IMapper mapper,
            ITranslationService translationService,
            IBadWordFilterService badWordFilterService,
            IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _translationService = translationService;
            _badWordFilterService = badWordFilterService;
            _redisHelper = redisHelper;
        }

        public async Task<ResponseModel> GetAsync(Guid id)
        {
            try
            {
                var cacheKey = $"package_{id}";
                return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                {
                    Func<IQueryable<Package>, IQueryable<Package>> include = services =>
                     services.Include(_ => _.PackageFeatures).ThenInclude(_ => _.Feature);

                    var package = await _unitOfWork.PackageRepository.GetAsync(id, include);
                    if (package == null)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status404NotFound,
                            Message = "Package not found."
                        };
                    }

                    var packageModel = new PackageModel()
                    {
                        Id = package.Id,
                        Name = package.Name,
                        Description = package.Description,
                        Price = package.Price,
                        DeliveryTime = package.DeliveryTime,
                        SketchRevision = package.SketchRevision,
                        ResponseTime = TimeSpan.FromMinutes(package.ResponseTime),
                        ServiceId = package.ServiceId,
                        IsDeleted = package.IsDeleted,
                        MinQuantity = package.MinQuantity,
                        MaxQuantity = package.MaxQuantity,
                        CreationDate = package.CreationDate,
                        Features = package.PackageFeatures
                            .GroupBy(pf => pf.Feature.Name)
                            .Select(g => new FeatureModel
                            {
                                Id = g.First().FeatureId,
                                Name = g.Key,
                                Question = g.First().Feature.Question,
                                QuestionType = g.First().Feature.QuestionType,
                                IsInformationRequired = g.First().Feature.IsInformationRequired,
                                IsQuantity = g.First().Feature.IsQuantity,
                                PackageFeatures = g.ToList()
                            }).ToList()
                    };

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully.",
                        Data = packageModel
                    };
                });
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

        public async Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { packageUpdateModel.Description };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var package = await _unitOfWork.PackageRepository.GetAsync(id);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                if (package.Name != packageUpdateModel.Name)
                {
                    var packageWithSameName = _unitOfWork.PackageRepository.GetPackageByNameAsync(packageUpdateModel.Name, (Guid)package.ServiceId!);
                    if (packageWithSameName)
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status422UnprocessableEntity,
                            Message = "Service already has this package's name."
                        };
                    }
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

                var packageModel = new PackageModel();
                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(id);

                if (!anyOrder.Result)
                {
                    _mapper.Map(packageUpdateModel, package);
                    //package.ResponseTime = (float)packageUpdateModel.ResponseTime.TotalMinutes;
                    _unitOfWork.PackageRepository.Update(package);
                    packageModel = _mapper.Map<PackageModel>(package);
                }
                else
                {
                    _unitOfWork.PackageRepository.SoftRemove(package);
                    Package newPackage = _mapper.Map<Package>(packageUpdateModel);
                    newPackage.ServiceId = package.ServiceId;
                    newPackage.Name = package.Name;
                    await _unitOfWork.PackageRepository.AddAsync(newPackage);
                    packageModel = _mapper.Map<PackageModel>(newPackage);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"package_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync($"service_{package.ServiceId}_packages_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Package updated successfully.",
                        Data = packageModel
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
                //await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                //await _unitOfWork.RollbackTransactionAsync();
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

                var packageFeatures = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                    filter: packageFeature => packageFeature.PackageId == id
                    );

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(id);

                if (!anyOrder.Result)
                {
                    _unitOfWork.PackageFeatureRepository.HardRemoveRange(packageFeatures.Data);
                    _unitOfWork.PackageRepository.HardRemove(package);
                }
                else
                {
                    _unitOfWork.PackageFeatureRepository.SoftRemoveRange(packageFeatures.Data);
                    _unitOfWork.PackageRepository.SoftRemove(package);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"package_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync($"services_{package.ServiceId}_packages_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Successfully delete."
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
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel> GetAllFeatureByPackageAsync(FeatureFilterModel model, Guid packageId, string sourceLanguageCode, string targetLanguage)
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

                //var cacheKey = $"package_{packageId}_features_{CacheTools.GenerateCacheKey(packageId)}";

                //return await _redisHelper.GetOrSetAsync(cacheKey, async () =>
                //{
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
                //});
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
                string[] fieldsToCheck = { packageFeatureAddModel.Name };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

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

                var numberOfExistedPackageFeature = _unitOfWork.PackageFeatureRepository.CountAvailablePackageFeaturesByPackage(package.Id);
                var maximumPackageFeature = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumPackageFeatureOfOnePackage).Result;
                if (numberOfExistedPackageFeature >= int.Parse(maximumPackageFeature!))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = $"Number of package features cannot exceed {maximumPackageFeature}."
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

                //var newPackageFeature = new PackageFeature
                //{
                //    //Name = sourceLanguageCode == "en" ? packageFeatureAddModel.Name : translatedName,
                //    Name = packageFeatureAddModel.Name,
                //    IsExtra = packageFeatureAddModel.IsExtra,
                //    AdditionalCost = packageFeatureAddModel.AdditionalCost,
                //    AdditionalDay = packageFeatureAddModel.AdditionalDay,
                //    MaxQuantity = packageFeatureAddModel.MaxQuantity,
                //    IsChecked = packageFeatureAddModel.IsChecked,
                //    FeatureId = feature.Id,
                //    PackageId = packageId
                //};

                var existingPackageFeatures = _unitOfWork.PackageFeatureRepository.GetAllAsync(
                    filter: _ => _.IsDeleted == false && _.PackageId == package.Id && _.FeatureId == feature.Id
                    ).Result.Data;

                if (packageFeatureAddModel.Index == 0)
                {
                    packageFeatureAddModel.Index = existingPackageFeatures.Count + 1;
                }
                else
                {
                    foreach (var existingPackageFeature in existingPackageFeatures)
                    {
                        if (existingPackageFeature.Index >= packageFeatureAddModel.Index)
                        {
                            existingPackageFeature.Index++;
                        }
                    }
                }
                _unitOfWork.PackageFeatureRepository.UpdateRange(existingPackageFeatures);

                var newPackageFeature = _mapper.Map<PackageFeature>(packageFeatureAddModel);
                newPackageFeature.FeatureId = feature.Id;
                newPackageFeature.PackageId = packageId;

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
                await _redisHelper.InvalidateCacheByPatternAsync($"package_{package.Id}");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{package.ServiceId}_packages_*");

                var packageFeatureModel = _mapper.Map<PackageFeatureModel>(newPackageFeature);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created.",
                    Data = packageFeatureModel
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

        public async Task<ResponseModel> AddRangePackageFeatureAsync(List<PackageFeatureAddModel> packageFeatureAddModels, Guid packageId, string sourceLanguageCode, string targetLanguageCode)
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

                var feature = await _unitOfWork.FeatureRepository.GetAsync(packageFeatureAddModels.First().FeatureId);
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                var existingPackageFeatures = _unitOfWork.PackageFeatureRepository.GetAllAsync(
                        filter: _ => _.IsDeleted == false && _.PackageId == package.Id && _.FeatureId == feature.Id
                        ).Result.Data;

                List<PackageFeature> packageFeatures = new List<PackageFeature>();

                foreach (var packageFeatureAddModel in packageFeatureAddModels)
                {
                    string[] fieldsToCheck = { packageFeatureAddModel.Name };

                    foreach (var field in fieldsToCheck)
                    {
                        ResponseModel response = sourceLanguageCode == "vi"
                            ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                            : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                        if (response.Code == StatusCodes.Status422UnprocessableEntity)
                            return response;
                    }

                    var numberOfExistedPackageFeature = _unitOfWork.PackageFeatureRepository.CountAvailablePackageFeaturesByPackage(package.Id);
                    var maximumPackageFeature = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumPackageFeatureOfOnePackage).Result;
                    if ((numberOfExistedPackageFeature += packageFeatureAddModels.Count()) > int.Parse(maximumPackageFeature!))
                    {
                        return new ResponseModel
                        {
                            Code = StatusCodes.Status422UnprocessableEntity,
                            Message = $"Number of package features cannot exceed {maximumPackageFeature}."
                        };
                    }

                    if (packageFeatureAddModel.Index == 0)
                    {
                        packageFeatureAddModel.Index = existingPackageFeatures.Count + 1;
                    }
                    else
                    {
                        foreach (var existingPackageFeature in existingPackageFeatures)
                        {
                            if (existingPackageFeature.Index >= packageFeatureAddModel.Index)
                            {
                                existingPackageFeature.Index++;
                            }
                        }
                    }

                    var newPackageFeature = _mapper.Map<PackageFeature>(packageFeatureAddModel);
                    newPackageFeature.FeatureId = feature.Id;
                    newPackageFeature.PackageId = packageId;

                    packageFeatures.Add(newPackageFeature);

                    existingPackageFeatures.Add(newPackageFeature);
                }

                existingPackageFeatures = existingPackageFeatures.Except(packageFeatures).ToList();

                _unitOfWork.PackageFeatureRepository.UpdateRange(existingPackageFeatures);
                await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);
                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

                await _redisHelper.InvalidateCacheByPatternAsync($"package_{package.Id}");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{package.ServiceId}_packages_*");

                var packageFeatureModel = _mapper.Map<List<PackageFeatureModel>>(packageFeatures);

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created.",
                    Data = packageFeatureModel
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
