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

        public FeatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> UpdateAsync(FeatureUpdateModel featureUpdateModel, Guid id)
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

                feature.Name = featureUpdateModel.Name;

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
