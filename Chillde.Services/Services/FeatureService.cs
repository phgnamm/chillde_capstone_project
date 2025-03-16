using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

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

                    if (response.Code != StatusCodes.Status200OK)
                        return response;
                }

                await _unitOfWork.BeginTransactionAsync();
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

                //var packageFeatures = new List<PackageFeature>();
                //for (int i = 0; i < featureAddModel.PackageFeatures.Count; i++)
                //{
                //    var packageFeature = featureAddModel.PackageFeatures[i];
                //    string translatedQuestion = translationResponse.TranslatedFields[$"PackageFeature_{i}_Name"];
                //    var newPackageFeature = new PackageFeature
                //    {
                //        Name = sourceLanguageCode == "en" ? packageFeature.Name : translatedQuestion,
                //        IsExtra = packageFeature.IsExtra,
                //        AdditionalCost = packageFeature.AdditionalCost,
                //        AdditionalDay = packageFeature.AdditionalDay,
                //        FeatureId = feature.Id,
                //        PackageId = packageId
                //    };
                //    packageFeatures.Add(newPackageFeature);
                //}
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

        public async Task<ResponseModel> UpdateAsync(FeatureUpdateModel featureUpdateModel, Guid id, string sourceLanguageCode)
        {
            try
            {
                string[] fieldsToCheck = { featureUpdateModel.Name, featureUpdateModel.Question };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code != StatusCodes.Status200OK)
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
                }

                _unitOfWork.FeatureRepository.Update(feature);
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
    }
}
