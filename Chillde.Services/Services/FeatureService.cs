using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.ServiceModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Nest;

namespace Chillde.Services.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranslationService _translationService;
        private readonly IMapper _mapper;
        private readonly IBadWordFilterService _badWordFilterService;

        public FeatureService(IUnitOfWork unitOfWork, ITranslationService translationService, IMapper mapper, IBadWordFilterService badWordFilterService)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
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

                var feature = new Feature
                {
                    //Name = sourceLanguageCode == "en" ? featureAddModel.Name : translatedName,
                    Name = featureAddModel.Name,
                    Question = featureAddModel.Question,
                    QuestionType = featureAddModel.QuestionType,
                    IsInformationRequired = featureAddModel.IsInformationRequired,
                    IsQuantity = featureAddModel.IsQuantity
                };

                await _unitOfWork.FeatureRepository.AddAsync(feature);

                var packageFeatures = new List<PackageFeature>();
                for (int i = 0; i < featureAddModel.PackageFeatureAddModels.Count; i++)
                {
                    var packageFeature = featureAddModel.PackageFeatureAddModels[i];
                    //string translatedQuestion = translationResponse.TranslatedFields[$"PackageFeature_{i}_Name"];
                    var newPackageFeature = new PackageFeature
                    {
                        //Name = sourceLanguageCode == "en" ? packageFeature.Name : translatedQuestion,
                        Name = packageFeature.Name,
                        AdditionalCost = packageFeature.AdditionalCost,
                        AdditionalDay = packageFeature.AdditionalDay,
                        IsExtra = packageFeature.IsExtra,
                        IsChecked = packageFeature.IsChecked,
                        MaxQuantity = packageFeature.MaxQuantity,
                        FeatureId = feature.Id,
                        PackageId = package.Id
                    };
                    packageFeatures.Add(newPackageFeature);
                }
                await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);

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
                featureModel.PackageFeatures = _mapper.Map<List<PackageFeature>>(packageFeatures);

                await _unitOfWork.SaveChangeAsync();
                //await _unitOfWork.CommitTransactionAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created.",

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

        public async Task<ResponseModel> UpdateAsync(FeatureUpdateModel featureUpdateModel, Guid id, string sourceLanguageCode, string targetLanguageCode)
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

                var feature = await _unitOfWork.FeatureRepository.GetAsync(id);
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                var anyOrder = _unitOfWork.FeatureRepository.HasAnyOrderByFeature(id);

                if (!anyOrder.Result)
                {
                    _mapper.Map(featureUpdateModel, feature);
                    _unitOfWork.FeatureRepository.Update(feature);
                }
                else
                {
                    _unitOfWork.FeatureRepository.SoftRemove(feature);
                    Feature newFeature = _mapper.Map<Feature>(featureUpdateModel);
                    await _unitOfWork.FeatureRepository.AddAsync(newFeature);

                    var packageFeatures = await _unitOfWork.PackageFeatureRepository.GetAllAsync(
                        filter: _ => _.FeatureId == feature.Id && _.IsDeleted == false
                        );

                    foreach (var packageFeature in packageFeatures.Data)
                    {
                        packageFeature.IsDeleted = true;
                    }
                    _unitOfWork.PackageFeatureRepository.UpdateRange(packageFeatures.Data);
                    await _unitOfWork.SaveChangeAsync();

                    foreach (var packageFeature in packageFeatures.Data)
                    {
                        packageFeature.Id = Guid.NewGuid();
                        packageFeature.IsDeleted = false;
                        packageFeature.FeatureId = newFeature.Id;
                    }
                    await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures.Data);

                    
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
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Feature successfully updated.",
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
