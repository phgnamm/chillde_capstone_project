using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Chillde.Services.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranslationService _translationService;
        private readonly IMapper _mapper;
        private readonly IBadWordFilterService _badWordFilterService;
        private readonly IPackageService _packageService;
        private readonly IRedisHelper _redisHelper;

        public FeatureService(IUnitOfWork unitOfWork, 
            ITranslationService translationService, 
            IMapper mapper, 
            IBadWordFilterService badWordFilterService, 
            IPackageService packageService, 
            IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
            _packageService = packageService;
            _redisHelper = redisHelper;
        }

        public async Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { featureAddModel.Name, featureAddModel.Question };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var package = await _unitOfWork.PackageRepository.GetAsync(featureAddModel.PackageId);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                var numberOfExistedPackageFeature = _unitOfWork.PackageFeatureRepository.CountAvailablePackageFeaturesByPackage(package.Id);
                var maximumPackageFeature = _unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MaximumPackageFeatureOfOnePackage).Result;
                if (numberOfExistedPackageFeature > int.Parse(maximumPackageFeature!))
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
                //    { "Name", featureAddModel.Name }
                //    };

                //var translationResponse = await _translationService.TranslateMultipleFieldsAsync(fieldsToTranslate, sourceLanguageCode, targetLanguageCode);
                //if (translationResponse.Code != StatusCodes.Status200OK)
                //{
                //    throw new Exception("Failed to translate fields.");
                //}

                //string translatedName = translationResponse.TranslatedFields["Name"];
                //var maximumFeature = _unitOfWork.SystemConfigRepository.GetByKeyAsync(SystemConfigKey.MaximumFeatureOfOnePackage).Result;
                //var numberOfExistedFeature = _unitOfWork.PackageRepository.GetAllPackageFromService(packageId).Result.Count();
                //if (numberOfExistedFeature > int.Parse(maximumFeature))
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status422UnprocessableEntity,
                //        Message = "Number of packages cannot exceed 3."
                //    };
                //}

                var existingFeatures = _unitOfWork.PackageRepository.GetAllFeatureByService((Guid)package.ServiceId!).Result;

                if (featureAddModel.Index == 0)
                {
                    featureAddModel.Index = existingFeatures.Count + 1;
                }
                else
                {
                    foreach (var existingFeature in existingFeatures)
                    {
                        if (existingFeature.Index >= featureAddModel.Index)
                        {
                            existingFeature.Index++;
                        }
                    }
                }
                _unitOfWork.FeatureRepository.UpdateRange(existingFeatures);

                var feature = _mapper.Map<Feature>(featureAddModel);

                await _unitOfWork.FeatureRepository.AddAsync(feature);
                await _unitOfWork.SaveChangeAsync();


                var packageFeatureAddModels = _mapper.Map<List<PackageFeatureAddModel>>(featureAddModel.PackageFeatureAddModels);
                packageFeatureAddModels.ForEach(_ => _.FeatureId = feature.Id);
                ResponseModel responseModel =  await _packageService.AddRangePackageFeatureAsync(packageFeatureAddModels, package.Id, sourceLanguageCode, targetLanguageCode);

                if (responseModel.Code != StatusCodes.Status201Created) 
                { 
                    _unitOfWork.FeatureRepository.HardRemove(feature);
                    await _unitOfWork.SaveChangeAsync();
                    return responseModel;
                }
                
                //await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);

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
                //if (!string.IsNullOrEmpty(featureAddModel.Name))
                //{
                //    translations.Add(new Translation
                //    {
                //        Id = Guid.NewGuid(),
                //        EntityType = "Feature",
                //        EntityId = feature.Id,
                //        FieldName = "Name",
                //        TranslationText = sourceLanguageCode != "en" ? featureAddModel.Name : translatedName,
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

                var featureModel = _mapper.Map<FeatureModel>(feature);
                featureModel.PackageFeatures = _mapper.Map<List<PackageFeature>>(packageFeatureAddModels);

                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created.",
                    Data = featureModel

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

        public async Task<ResponseModel> UpdateAsync(FeatureUpdateModelForFeatureService featureUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { featureUpdateModel.Name, featureUpdateModel.Question };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var feature = await _unitOfWork.FeatureRepository.GetAsync(id, 
                    include: feature => feature.Include(_ => _.PackageFeatures)
                                               .ThenInclude(_ => _.Package));
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                var serviceId = feature.PackageFeatures.FirstOrDefault().Package.ServiceId;

                var existingFeatures = _unitOfWork.PackageRepository.GetAllFeatureByService((Guid)serviceId).Result;

                if (featureUpdateModel.Index == 0)
                {
                    featureUpdateModel.Index = feature.Index;
                }
                else
                {
                    foreach (var existingFeature in existingFeatures)
                    {
                        if (existingFeature.Index >= featureUpdateModel.Index && existingFeature.Index < feature.Index)
                        {
                            existingFeature.Index++;
                        }
                    }
                }
                _unitOfWork.FeatureRepository.UpdateRange(existingFeatures);

                FeatureModel featureModel = new FeatureModel();

                var anyOrder = _unitOfWork.FeatureRepository.HasAnyOrderByFeature(id);

                if (!anyOrder.Result)
                {
                    _mapper.Map(featureUpdateModel, feature);
                    _unitOfWork.FeatureRepository.Update(feature);
                    featureModel = _mapper.Map<FeatureModel>(feature);
                }
                else
                {
                    _unitOfWork.FeatureRepository.SoftRemove(feature);
                    Feature newFeature = _mapper.Map<Feature>(featureUpdateModel);
                    await _unitOfWork.FeatureRepository.AddAsync(newFeature);

                    var packageFeatures = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                        filter: _ => _.FeatureId == feature.Id && _.IsDeleted == false
                        );

                    //foreach (var packageFeature in packageFeatures.Data)
                    //{
                    //    packageFeature.IsDeleted = true;
                    //}
                    _unitOfWork.PackageFeatureRepository.SoftRemoveRange(packageFeatures.Data);
                    await _unitOfWork.SaveChangeAsync();

                    foreach (var packageFeature in packageFeatures.Data)
                    {
                        packageFeature.Id = Guid.NewGuid();
                        packageFeature.IsDeleted = false;
                        packageFeature.FeatureId = newFeature.Id;
                    }
                    await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures.Data);
                    featureModel = _mapper.Map<FeatureModel>(newFeature);
                    featureModel.PackageFeatures = packageFeatures.Data;

                    //for (int i = 0; i < packageFeatures.TotalCount; i++)
                    //{
                    //    var packageFeature = featureUpdateModel.PackageFeatureAddModels[i];
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
                    //        FeatureId = feature.Id,
                    //        PackageId = package.Id
                    //    };
                    //    packageFeatures.Add(newPackageFeature);
                    //}

                }
                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"features_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("features_*");
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Feature successfully updated.",
                        Data = featureModel
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

        public async Task<ResponseModel> DeleteAsync(Guid id)
        {
            try
            {
                var feature = await _unitOfWork.FeatureRepository.GetAsync(id);
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                var packageFeatures = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                        filter: _ => _.FeatureId == feature.Id && _.IsDeleted == false
                        );

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByFeature(id);

                if (!anyOrder.Result)
                {
                    _unitOfWork.PackageFeatureRepository.HardRemoveRange(packageFeatures.Data);
                    _unitOfWork.FeatureRepository.HardRemove(feature);
                }
                else
                {
                    _unitOfWork.FeatureRepository.SoftRemove(feature);

                    foreach (var packageFeature in packageFeatures.Data)
                    {
                        packageFeature.IsDeleted = true;
                    }
                    _unitOfWork.PackageFeatureRepository.UpdateRange(packageFeatures.Data);
                }

                var changes = await _unitOfWork.SaveChangeAsync();
                if (changes > 0)
                {
                    await _redisHelper.InvalidateCacheByPatternAsync($"features_{id}");
                    await _redisHelper.InvalidateCacheByPatternAsync("features_*");
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
    }
}
