using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.CategoryModels;
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageFeatureModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.CategoryModels;
using Chillde.Services.Models.FeatureModels;
using Chillde.Services.Models.PackageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PackageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> UpdateAsync(PackageUpdateModel packageUpdateModel, Guid id)
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

                package.Price = packageUpdateModel.Price;

                _unitOfWork.PackageRepository.Update(package);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Package updated successfully.",
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

        public async Task<ResponseModel> AddFeatureAsync(FeatureAddModel featureAddModel, Guid packageId)
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
                    Name = featureAddModel.Name
                };

                await _unitOfWork.FeatureRepository.AddAsync(feature);

                var packageFeatures = new List<PackageFeature>();
                foreach (var packageFeature in featureAddModel.PackageFeatures)
                {
                    var newPackageFeature = new PackageFeature
                    {
                        Question = packageFeature.Question,
                        IsInformationRequired = packageFeature.IsInformationRequired,
                        IsExtra = packageFeature.IsExtra,
                        AdditionalCost = packageFeature.AdditionalCost,
                        AdditionalDay = packageFeature.AdditionalDay,
                        FeatureId = feature.Id,
                        PackageId = packageId
                    };
                    packageFeatures.Add(newPackageFeature);
                }

                await _unitOfWork.PackageFeatureRepository.AddRangeAsync(packageFeatures);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully created."
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

        public async Task<ResponseModel> GetAllFeatureAsync(FeatureFilterModel model, Guid packageId)
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

                var featureModels = _mapper.Map<List<FeatureModel>>(features.Data);

                var result = new Pagination<FeatureModel>(featureModels, model.PageIndex,
                    model.PageSize, features.TotalCount);

                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Successfully.",
                    Data = result
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
