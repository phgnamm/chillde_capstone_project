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

        public async Task<ResponseModel> AddRangeAsync(Guid serviceId, ServiceAttachmentAddModel model)
        {
            try
            {
                //if (model.AttachmentAlt.Count != model.AttachmentUrls!.Count)
                //{
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status400BadRequest,
                //        Message = "The number of attachments and images must match."
                //    };
                //}

                //var newServiceAttachment = new List<ServiceAttachment>();

                //for (int i = 0; i < model.AttachmentAlt.Count; i++)
                //{
                //    var attachmentAlt = model.AttachmentAlt[i];
                //    var attachmentUrl = model.AttachmentUrls[i];

                //    string? path = null;
                //    if (attachmentUrl != null)
                //    {
                //        path = await _cloudinaryHelper.UploadImageAsync(
                //            attachmentUrl,
                //            "serviceAttachments",
                //            Guid.NewGuid().ToString()
                //        );
                //    }

                //    newServiceAttachment.Add(new ServiceAttachment
                //    {
                //        AttachmentAlt = attachmentAlt,
                //        AttachmentUrl = path,
                //        ServiceId = serviceId,
                //    });
                //}


                //await _unitOfWork.ServiceAttachmentRepository.AddRangeAsync(newServiceAttachment);
                //await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Successfully.",
                    //Data = newServiceAttachment
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
