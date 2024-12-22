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

        public async Task<ResponseModel> Add(FeedbackAddModel feedbackAddModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized."
                };
            }

            var hasCompletedOrder = await _unitOfWork.OrderRepository.HasCompletedOrder(currentUserId.Value, feedbackAddModel.ServiceId);
            if (!hasCompletedOrder)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "User has not completed an order in this service."
                };
            }
/*
            var hasFeedback = await _unitOfWork.FeedbackRepository.HasFeedback(currentUserId.Value, feedbackAddModel.ServiceId);
            if (hasFeedback)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "User has already given feedback for this service."
                };
            }*/

            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                ServiceId = feedbackAddModel.ServiceId,
                CreatedById = currentUserId.Value,
                Rating = feedbackAddModel.Rating,
                Description = feedbackAddModel.Description,
            };

            await _unitOfWork.FeedbackRepository.AddAsync(feedback);
            if (feedbackAddModel.FeedbackAttachmentAddModels != null && feedbackAddModel.FeedbackAttachmentAddModels.Count > 0)
            {
                var feedbackAttachments = new List<FeedbackAttachment>();

                foreach (var attachment in feedbackAddModel.FeedbackAttachmentAddModels)
                {
                    var attachmentPath = await _cloudinaryHelper.UploadImageAsync(
                        attachment.AttachmentUrl,
                        "feedbacks",
                        feedback.Id.ToString()
                    );

                    feedbackAttachments.Add(new FeedbackAttachment
                    {
                        Id = Guid.NewGuid(),
                        FeedbackId = feedback.Id,
                        AttachmentUrl = attachmentPath,
                        AttachmentAlt = attachment.AttachmentAlt,
                    });
                }

                await _unitOfWork.FeedbackAttachmentRepository.AddRangeAsync(feedbackAttachments);
            }

            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel
            {
                Code = StatusCodes.Status201Created,
                Message = "Feedback created successfully.",
            };
        }


        public async Task<ResponseModel> GetAllByService(Guid serviceId, FeedbackFilterModel feedbackFilterModel)
        {
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAllAsync(
                filter: _ => _.IsDeleted == feedbackFilterModel.IsDeleted,
                include: feedbacks => feedbacks.Include(_ => _.FeedbackAttachments) 
                                               .Include(_ => _.CreatedBy) 
                                               .Include(_ => _.Service),
                pageIndex: feedbackFilterModel.PageIndex,
                pageSize: feedbackFilterModel.PageSize
            );
            var feedbackModels = feedbacks.Data.Select(_ => new FeedbackModel
            {
                Id = _.Id,
                CreatedById = _.CreatedById,
                AuthorName = _.CreatedBy.FirstName + " " + _.CreatedBy.LastName,
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackImageModels = _.FeedbackAttachments.Select(_ => new FeedbackImageModel
                {
                    ImageUrl = _.AttachmentUrl ?? ""
                }).ToList()
            }).ToList();
            var result = new Pagination<FeedbackModel>(feedbackModels, feedbackFilterModel.PageIndex,
                feedbackFilterModel.PageSize, feedbacks.TotalCount);

            return new ResponseModel
            {
                Message = "Get all feedbacks successfully",
                Data = result
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

    }
}
