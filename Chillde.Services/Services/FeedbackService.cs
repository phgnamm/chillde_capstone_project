using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.AccountModels;
using Chillde.Repositories.Models.FeedbackModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.FeedbackModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Linq;
using System.Linq.Expressions;


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
            var feedbacks = await _unitOfWork.FeedbackRepository.GetAsync(id, _ => _.Where(_ => _.Id == id).Include(_ => _.FeedbackAttachments).Include(_ => _.CreatedBy));

            var feedbackModels = new FeedbackModel
            {
                Id = feedbacks!.Id,
                CreatedById = feedbacks.CreatedById,
                ServiceId = feedbacks.ServiceId,
                CreatedBy = new AccountLiteModel
                {
                    FirstName = feedbacks.CreatedBy.FirstName,
                    LastName = feedbacks.CreatedBy.LastName,
                    Username = feedbacks.CreatedBy.Username,
                    Email = feedbacks.CreatedBy.Email,
                    Image = feedbacks.CreatedBy.Image
                },
                Description = feedbacks.Description,
                CreationDate = feedbacks.CreationDate,
                Rating = feedbacks.Rating,
                Response = feedbacks.Response,
                FeedbackAttachmentModels = feedbacks.FeedbackAttachments.Select(_ => new FeedbackAttachmentModel
                {
                    AttachmentAlt = _.AttachmentAlt,
                    AttachmentUrl = _.AttachmentUrl,
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

            var existingFeedback = await _unitOfWork.FeedbackRepository.GetAsync(id, _ => _.Include(_ => _.FeedbackAttachments).Include(_ => _.Service));
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
            existingFeedback.Description = feedbackUpdateModel.Description ?? existingFeedback.Description;
            existingFeedback.Response = existingFeedback.Response ?? feedbackUpdateModel.Response;
            existingFeedback.ModificationDate = DateTime.UtcNow;
            if (feedbackUpdateModel.Rating.HasValue && feedbackUpdateModel.Rating.Value != existingFeedback.Rating)
            {
                var oldRating = existingFeedback.Rating ?? 0;
                var newRating = feedbackUpdateModel.Rating.Value;
                var currentRate = existingFeedback.Service.Rate ?? 0;
                var feedbackCount = existingFeedback.Service.FeedbackCount ?? 0;
                var newRate = RecalculateRating(currentRate, feedbackCount, oldRating, newRating);
                existingFeedback.Service.Rate = newRate;
                _unitOfWork.ServiceRepository.Update(existingFeedback.Service);

                existingFeedback.Rating = newRating;
            }
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
        private static double RecalculateRating(double currentAverage, int count, double oldRating, double newRating)
        {
            var adjustedTotal = currentAverage * count - oldRating + newRating;
            var newAverage = adjustedTotal / count;
            return Math.Round(newAverage, 1);
        }

        public async Task<ResponseModel> GetAllFeedbacksByArtisanAsync(Guid accountId, FeedbackFilterModel feedbackFilterModel)
        {
            var account = await _unitOfWork.AccountRepository.GetAsync(
                accountId,
                query => query.Include(a => a.AccountRoles)
            );
            if (account == null || !account.AccountRoles.Any(r => r.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString()))
            {
                return new ResponseModel
                {
                    Message = "Account is not an Artisan or does not exist",
                    Data = null
                };
            }

            var servicesResult = await _unitOfWork.ServiceRepository.GetAllAsync(
                filter: s => s.CreatedById == accountId && !s.IsDeleted
            );
            var serviceIds = servicesResult.Data.Select(s => s.Id).ToList();

            if (!serviceIds.Any())
            {
                return new ResponseModel
                {
                    Message = "No services found for this artisan",
                    Data = new Pagination<FeedbackModel>(
                        new List<FeedbackModel>(),
                        feedbackFilterModel.PageIndex,
                        feedbackFilterModel.PageSize,
                        0
                    )
                };
            }

            Expression<Func<Feedback, bool>> filter = feedback =>
                serviceIds.Contains(feedback.ServiceId) &&
                (feedback.IsDeleted == feedbackFilterModel.IsDeleted) &&
                (
                    (feedbackFilterModel.OneStar == true && feedback.Rating == 1) ||
                    (feedbackFilterModel.TwoStar == true && feedback.Rating == 2) ||
                    (feedbackFilterModel.ThreeStar == true && feedback.Rating == 3) ||
                    (feedbackFilterModel.FourStar == true && feedback.Rating == 4) ||
                    (feedbackFilterModel.FiveStar == true && feedback.Rating == 5) ||
                    (
                        feedbackFilterModel.OneStar == null &&
                        feedbackFilterModel.TwoStar == null &&
                        feedbackFilterModel.ThreeStar == null &&
                        feedbackFilterModel.FourStar == null &&
                        feedbackFilterModel.FiveStar == null
                    )
                );

            var feedbacksResult = await _unitOfWork.FeedbackRepository.GetAllAsync(
                filter: filter,
                include: f => f.Include(_ => _.FeedbackAttachments)
                               .Include(_ => _.CreatedBy)
                               .Include(_ => _.Service),
                order: _ =>
                {
                    switch (feedbackFilterModel.Order.ToLower())
                    {
                        case "star":
                            return feedbackFilterModel.OrderByDescending
                                ? _.OrderByDescending(s => s.Rating)
                                : _.OrderBy(s => s.Rating);
                        default:
                            return feedbackFilterModel.OrderByDescending
                                ? _.OrderByDescending(s => s.CreationDate)
                                : _.OrderBy(s => s.CreationDate);
                    }
                },
                pageIndex: feedbackFilterModel.PageIndex,
                pageSize: feedbackFilterModel.PageSize
            );

            var feedbackModels = feedbacksResult.Data.Select(_ => new FeedbackModel
            {
                Id = _.Id,
                CreatedById = _.CreatedById,
                ServiceId = _.ServiceId,
                CreatedBy = new AccountLiteModel
                {
                    FirstName = _.CreatedBy.FirstName,
                    LastName = _.CreatedBy.LastName,
                    Username = _.CreatedBy.Username,
                    Email = _.CreatedBy.Email,
                    Image = _.CreatedBy.Image
                },
                Description = _.Description,
                CreationDate = _.CreationDate,
                Rating = _.Rating,
                FeedbackAttachmentModels = _.FeedbackAttachments.Select(a => new FeedbackAttachmentModel
                {
                    AttachmentAlt = a.AttachmentAlt,
                    AttachmentUrl = a.AttachmentUrl
                }).ToList()
            }).ToList();

            var result = new Pagination<FeedbackModel>(
                feedbackModels,
                feedbackFilterModel.PageIndex,
                feedbackFilterModel.PageSize,
                feedbacksResult.TotalCount
            );

            return new ResponseModel
            {
                Message = "Get all feedbacks for artisan successfully",
                Data = result
            };
        }
    }
}
