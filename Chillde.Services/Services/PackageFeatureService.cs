using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FAQModels;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageFeatureModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceModels;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class PackageFeatureService : IPackageFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITranslationService _translationService;

        public PackageFeatureService(IUnitOfWork unitOfWork, IMapper mapper, ITranslationService translationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _translationService = translationService;
        }

        public async Task<ResponseModel> UpdateAsync(PackageFeatureUpdateModel packageFeatureUpdateModel, Guid id)
        {
            try
            {
                var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(id);
                if (packageFeature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package feature not found."
                    };
                }

                var anyOrder = await _unitOfWork.OrderRepository.HasAnyOrderByPackage(packageFeature.PackageId!.Value);

                if (!anyOrder)
                {
                    _mapper.Map(packageFeatureUpdateModel, packageFeature);
                    _unitOfWork.PackageFeatureRepository.Update(packageFeature);
                }
                else
                {
                    _unitOfWork.PackageFeatureRepository.SoftRemove(packageFeature);
                    PackageFeature newPackageFeature = _mapper.Map<PackageFeature>(packageFeatureUpdateModel);
                    newPackageFeature.PackageId = packageFeature.PackageId;
                    newPackageFeature.FeatureId = packageFeature.FeatureId;
                    await _unitOfWork.PackageFeatureRepository.AddAsync(newPackageFeature);
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

        public async Task<ResponseModel> DeletePackageFeatureAsync(Guid packageFeatureId)
        {
            try
            {
                var packageFeature = await _unitOfWork.PackageFeatureRepository.GetAsync(packageFeatureId);
                if (packageFeature == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Package feature not found."
                    };
                }

                var anyOrder = _unitOfWork.OrderRepository.HasAnyOrderByPackage(packageFeature.PackageId!.Value);

                if (!anyOrder.Result)
                {
                    _unitOfWork.PackageFeatureRepository.HardRemove(packageFeature);
                }
                else
                {
                    _unitOfWork.PackageFeatureRepository.SoftRemove(packageFeature);
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