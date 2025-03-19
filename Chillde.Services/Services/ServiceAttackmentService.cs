using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.ServiceAttachmentModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class ServiceAttackmentService : IServiceAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IMapper _mapper;

        public ServiceAttackmentService(IUnitOfWork unitOfWork, ICloudinaryHelper cloudinaryHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryHelper = cloudinaryHelper;
            _mapper = mapper;
        }

        public async Task<ResponseModel> DeleteServiceAttachmentAsync(List<Guid> serviceAttacchmentIds)
        {
            try
            {
                var serviceAttachments = await _unitOfWork.ServiceAttachmentRepository.GetAllAsync(
                    filter: _ => serviceAttacchmentIds.Contains(_.Id)
                    );

                var a = serviceAttachments.Data;

                if (serviceAttachments == null || !serviceAttachments.Data.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Attachments not found."
                    };
                }

                var publicIds = serviceAttachments.Data.Select(a => a.Id).ToList();

                await _cloudinaryHelper.RemoveImagesAsync(serviceAttacchmentIds.Select(id => id.ToString()).ToList());

                _unitOfWork.ServiceAttachmentRepository.HardRemoveRange(serviceAttachments.Data);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Success"
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
