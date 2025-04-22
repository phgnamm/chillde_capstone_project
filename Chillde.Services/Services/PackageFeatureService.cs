using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Services.Services
{
    public class PackageFeatureService : IPackageFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITranslationService _translationService;
        private readonly IBadWordFilterService _badWordFilterService;
        private readonly IRedisHelper _redisHelper;

        public PackageFeatureService(IUnitOfWork unitOfWork, IMapper mapper, ITranslationService translationService, IBadWordFilterService badWordFilterService, IRedisHelper redisHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _translationService = translationService;
            _badWordFilterService = badWordFilterService;
            _redisHelper = redisHelper;
        }

        public async Task<ResponseModel> UpdateAsync(PackageFeatureUpdateModel packageFeatureUpdateModel, Guid id, string sourceLanguageCode)
        {
            try
            {
                    string[] fieldsToCheck = { packageFeatureUpdateModel.Name };

                foreach (var field in fieldsToCheck)
                {
                    ResponseModel response = sourceLanguageCode == "vi"
                        ? await _badWordFilterService.FilterVietnameseBadWordsAsync(field)
                        : await _badWordFilterService.FilterEnglishBadWordsAsync(field);

                    if (response.Code == StatusCodes.Status422UnprocessableEntity)
                        return response;
                }

                var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(id);
                if (packageFeature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package feature not found."
                    };
                }

                var package = await _unitOfWork.PackageRepository.GetAsync((Guid)packageFeatureUpdateModel.PackageId);
                if (package == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package not found."
                    };
                }

                PackageFeature newPackageFeature = new PackageFeature();

                var existingPackageFeatures = _unitOfWork.PackageFeatureRepository.GetAllAsync(
                    filter: _ => _.IsDeleted == false && _.PackageId == packageFeatureUpdateModel.PackageId && _.FeatureId == packageFeature.FeatureId
                    ).Result.Data;

                if (packageFeatureUpdateModel.Index == 0)
                {
                    packageFeatureUpdateModel.Index = packageFeature.Index;
                }
                else
                {
                    foreach (var existingPackageFeature in existingPackageFeatures)
                    {
                        if (existingPackageFeature.Index >= packageFeatureUpdateModel.Index && existingPackageFeature.Index < packageFeature.Index)
                        {
                            existingPackageFeature.Index++;
                        }
                    }
                }
                _unitOfWork.PackageFeatureRepository.UpdateRange(existingPackageFeatures);

                PackageFeatureModel newPackageFeatureModel = new PackageFeatureModel();
                var anyOrder = await _unitOfWork.OrderRepository.HasAnyOrderByPackage(packageFeature.PackageId!.Value);

                if (!anyOrder)
                {
                    _mapper.Map(packageFeatureUpdateModel, packageFeature);
                    _unitOfWork.PackageFeatureRepository.Update(packageFeature);
                    newPackageFeatureModel = _mapper.Map<PackageFeatureModel>(packageFeature);
                }
                else
                {
                    _unitOfWork.PackageFeatureRepository.SoftRemove(packageFeature);
                    newPackageFeature = _mapper.Map<PackageFeature>(packageFeatureUpdateModel);
                    newPackageFeature.PackageId = packageFeature.PackageId;
                    newPackageFeature.FeatureId = packageFeature.FeatureId;
                    await _unitOfWork.PackageFeatureRepository.AddAsync(newPackageFeature);
                    newPackageFeatureModel = _mapper.Map<PackageFeatureModel>(newPackageFeature);
                }

                //var newPackageFeatureModel = _mapper.Map<PackageFeatureModel>(newPackageFeature);

                await _unitOfWork.SaveChangeAsync();

                await _redisHelper.InvalidateCacheByPatternAsync($"package_{package.Id}");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{package.ServiceId}_packages_*");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{package.ServiceId}_features_*");

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Package feature successfully updated.",
                    Data = newPackageFeatureModel
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

        public async Task<ResponseModel> DeletePackageFeatureAsync(Guid packageFeatureId)
        {
            try
            {
                var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(packageFeatureId, 
                    include: packageFeature => packageFeature.Include(_ => _.Package));
                if (packageFeature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package feature not found."
                    };
                }

                var feature = await _unitOfWork.FeatureRepository.GetAsync(packageFeature.FeatureId);
                if (feature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Feature not found."
                    };
                }

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(packageFeature.PackageId!.Value);

                if (!anyOrder.Result)
                {
                    _unitOfWork.PackageFeatureRepository.HardRemove(packageFeature);
                    int availablePackageFeatures = _unitOfWork.PackageFeatureRepository.CountAvailablePackageFeaturesByFeature(packageFeature.FeatureId);
                    if (availablePackageFeatures == 1)
                    {
                        await _unitOfWork.FeatureRepository.GetAsync(packageFeature.FeatureId);
                        _unitOfWork.FeatureRepository.HardRemove(feature);
                    }
                }
                else
                {
                    _unitOfWork.PackageFeatureRepository.SoftRemove(packageFeature);                    
                }

                await _unitOfWork.SaveChangeAsync();

                await _redisHelper.InvalidateCacheByPatternAsync($"package_{packageFeature.PackageId}");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{packageFeature.Package.ServiceId}_packages_*");
                await _redisHelper.InvalidateCacheByPatternAsync($"service_{packageFeature.Package.ServiceId}_features_*");

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