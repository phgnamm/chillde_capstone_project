using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.FeedbackModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace Chillde.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public FeedbackService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> GetById(Guid id)
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAsync(id, _ => _.Where(_ => _.Id == id).Include(_ => _.FeedbackAttachments));

            var feedbackModels = new FeedbackModel
            {
                Id = feedbacks!.Id,
                CreatedById = feedbacks.CreatedById,
                ServiceId = feedbacks.ServiceId,
                AuthorName = feedbacks.CreatedBy.FirstName + " " + feedbacks.CreatedBy.LastName,
                Description = feedbacks.Description,
                CreationDate = feedbacks.CreationDate,
                Rating = feedbacks.Rating,
                Response = feedbacks.Response,
                FeedbackImageModels = feedbacks.FeedbackAttachments.Select(_ => new FeedbackImageModel
                {
                    ImageUrl = _.AttachmentUrl ?? ""
                }).ToList()
            };
            return new ResponseModel
            {
                Data = feedbackModels,
                Message = "Get feedback successfully"
            };
        }

        public async Task<ResponseModel> Update(Guid id, FeedbackUpdateModel feedbackUpdateModel)
        {
            if (feedbackUpdateModel == null || id == Guid.Empty)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid input."
                };
            }

            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }

            var existingFeedback = await _unitOfWork.FeedbackRepository.GetAsync(id, _ => _.Include(_ => _.FeedbackAttachments));
            if (existingFeedback == null || existingFeedback.IsDeleted)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Feedback not found."
                };
            }

            if (existingFeedback.CreatedById != currentUserId.Value)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "You are not authorized to update this feedback."
                };
            }
            if ((DateTime.UtcNow - existingFeedback.CreationDate).TotalDays > 30)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Feedback cannot be updated after 30 days from its creation date."
                };
            }
            existingFeedback.Rating = feedbackUpdateModel.Rating ?? existingFeedback.Rating;
            existingFeedback.Description = feedbackUpdateModel.Description ?? existingFeedback.Description;
            existingFeedback.Response = existingFeedback.Response ?? feedbackUpdateModel.Response;
            existingFeedback.ModificationDate = DateTime.UtcNow;

            _unitOfWork.FeedbackRepository.Update(existingFeedback);
            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Feedback updated successfully."
                }
                : new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Failed to update feedback."
                };
        }
        public async Task<ResponseModel> RespondToFeedback(Guid feedbackId, string responseText)
        {
            var existingFeedback = await _unitOfWork.FeedbackRepository.GetAsync(feedbackId);
            if (existingFeedback == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Feedback not found."
                };
            }

            existingFeedback.Response = responseText;
            _unitOfWork.FeedbackRepository.Update(existingFeedback);
            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Response added successfully."
                }
                : new ResponseModel
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Failed to add response."
                };
        }

    }
}
