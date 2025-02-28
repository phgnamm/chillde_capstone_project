using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Services.Services
{
    public class RequestAttributeService : IRequestAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestAttributeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> RemoveAttribute(Guid id)
        {
            var attribute = await _unitOfWork.RequestAttributeRepository.GetAsync(id,
                _ => _.Include(_ => _.RequestAttributeValues)
                      .Include(_ => _.RequestAttributeAttachments));

            if (attribute == null)
            {
                return new ResponseModel { Message = "Attribute not found.", Code = StatusCodes.Status404NotFound };
            }

            if (attribute.RequestAttributeValues.Any())
            {
                _unitOfWork.RequestAttributeValueRepository.HardRemoveRange(attribute.RequestAttributeValues.ToList());
            }

            if (attribute.RequestAttributeAttachments.Any())
            {
                _unitOfWork.RequestAttributeAttachmentRepository.HardRemoveRange(attribute.RequestAttributeAttachments.ToList());
            }

            _unitOfWork.RequestAttributeRepository.HardRemove(attribute);

            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel { Message = "Delete attribute successfully" }
                : new ResponseModel { Message = "Delete attribute unsuccessfully", Code = StatusCodes.Status400BadRequest };
        }
        public async Task<ResponseModel> RemoveAttributeValue(Guid id)
        {
            var findInAttributeValue = await _unitOfWork.RequestAttributeValueRepository
                .GetAsync(id, include: _ => _.Include(_ => _.RequestAttribute));

            var findInAttributeAttachment = await _unitOfWork.RequestAttributeAttachmentRepository
                .GetAsync(id, include: _ => _.Include(_ => _.RequestAttribute));

            var requestAttribute = findInAttributeValue?.RequestAttribute ?? findInAttributeAttachment?.RequestAttribute;
            if (requestAttribute == null)
            {
                return new ResponseModel { Message = "Attribute value not found.", Code = StatusCodes.Status404NotFound };
            }

            if (findInAttributeValue != null)
            {
                _unitOfWork.RequestAttributeValueRepository.HardRemove(findInAttributeValue);
            }
            if (findInAttributeAttachment != null)
            {
                _unitOfWork.RequestAttributeAttachmentRepository.HardRemove(findInAttributeAttachment);
            }

            var remainingValues = await _unitOfWork.RequestAttributeValueRepository
                .GetAllAsync(filter: _ => _.RequestAttributeId == requestAttribute.Id);
            var remainingAttachments = await _unitOfWork.RequestAttributeAttachmentRepository
                .GetAllAsync(filter: _ => _.RequestAttributeId == requestAttribute.Id);

            if (!remainingValues.Data.Any() && !remainingAttachments.Data.Any())
            {
                _unitOfWork.RequestAttributeRepository.HardRemove(requestAttribute);
            }

            var result = await _unitOfWork.SaveChangeAsync();
            return result > 0
                ? new ResponseModel { Message = "Delete value successfully" }
                : new ResponseModel { Message = "Delete value unsuccessfully", Code = StatusCodes.Status400BadRequest };
        }


    }
}
