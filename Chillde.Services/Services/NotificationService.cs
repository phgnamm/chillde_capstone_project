using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Services.Common;
using Chillde.Services.Hubs;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.NotificationModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chillde.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IClaimService _claimService;
        private readonly ConnectionMapping<Guid> _connections = new();
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IRedisHelper _redisHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryHelper _cloudinaryHelper;

        public NotificationService(IClaimService claimService, IMapper mapper,
            IRedisHelper redisHelper, IUnitOfWork unitOfWork, IHubContext<RealTimeHub> hubContext,
            ICloudinaryHelper cloudinaryHelper)
        {
            _claimService = claimService;
            _mapper = mapper;
            _redisHelper = redisHelper;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _cloudinaryHelper = cloudinaryHelper;
        }

        public async Task<ResponseModel> GetAll(NotificationFilterModel notificationFilterModel)
        {

            Func<IQueryable<Notification>, IOrderedQueryable<Notification>> orderBy = query =>
            {
                switch (notificationFilterModel.Order?.ToLower())
                {
                    case "creationdate":
                        return notificationFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                    default:
                        return notificationFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                }
            };

            Expression<Func<Notification, bool>> filter = notification =>
                (!notificationFilterModel.AccountId.HasValue || notification.AccountId == notificationFilterModel.AccountId) &&
                (!notificationFilterModel.NotificationType.HasValue || notification.NotificationContent.Type == notificationFilterModel.NotificationType);

            try
            {
                var notifications = await _unitOfWork.NotificationRepository.GetAllAsync(
                    filter: filter,
                    include: notification => notification.Include(o => o.NotificationContent),
                    order: orderBy,
                    pageIndex: notificationFilterModel.PageIndex,
                    pageSize: notificationFilterModel.PageSize
                );

                var notificationModels = _mapper.Map<List<NotificationModel>>(notifications.Data);

                var result = new Pagination<NotificationModel>(
                    notificationModels,
                    notificationFilterModel.PageIndex,
                    notificationFilterModel.PageSize,
                    notifications.TotalCount
                );

                return new ResponseModel
                {
                    Message = "Get all reports successfully",
                    Data = result,

                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred."
                };
            }
        }

        public async Task<ResponseModel> UpdateIsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _unitOfWork.NotificationRepository.GetAsync(notificationId);

                if (notification == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Notification not found."
                    };
                }

                notification.IsRead = true;

                _unitOfWork.NotificationRepository.Update(notification);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseModel
                {
                    Message = "Notification updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while updating the notification."
                };
            }
        }


        public async Task<ResponseModel> PushNotification(NotificationAddModel notificationAddModel)
        {
            if (string.IsNullOrWhiteSpace(notificationAddModel.Content))
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Please provide content or attachment"
                };
            }

            //var currentUserId = _claimService.GetCurrentUserId;
            //if (!currentUserId.HasValue)
            //    return new ResponseModel
            //    {
            //        Code = StatusCodes.Status401Unauthorized,
            //        Message = "Unauthorized"
            //    };

            var notification = _mapper.Map<Notification>(notificationAddModel);

            //var accountConversations = new List<AccountConversation>();
            //foreach (var accountConversation in existedConversation.AccountConversations)
            //{
            //    if (accountConversation.IsArchived)
            //    {
            //        accountConversation.IsArchived = false;
            //        accountConversations.Add(accountConversation);
            //    }

            //    message.MessageRecipients.Add(new MessageRecipient
            //    {
            //        IsRead = accountConversation.AccountId == currentUserId,
            //        AccountId = accountConversation.AccountId,
            //        AccountConversationId = accountConversation.Id
            //    });
            //}

            //_unitOfWork.AccountConversationRepository.UpdateRange(accountConversations);
            await _unitOfWork.NotificationRepository.AddAsync(notification);
            if (await _unitOfWork.SaveChangeAsync() > 0)
            {
                //var recipientId = notification.AccountId;
                // foreach (var recipientId in recipientIds)
                // {
                // var updateConversation =
                //     await _unitOfWork.ConversationRepository.FindByAccountIdAndConversationIdAsync(recipientId,
                //         conversationId, conversations => conversations
                //             .Include(conversation => conversation.AccountConversations)
                //             .ThenInclude(accountConversation => accountConversation.Account)
                //             .Include(conversation => conversation.AccountConversations).ThenInclude(
                //                 accountConversation =>
                //                     accountConversation.MessageRecipients
                //                         .Where(messageRecipient => !messageRecipient.IsDeleted)
                //                         .OrderByDescending(messageRecipient => messageRecipient.Message.CreationDate)
                //                         .Take(6))
                //             .ThenInclude(messageRecipient => messageRecipient.Message));
                //     await _hubContext.Clients
                //         .Clients(_connections.GetConnections(recipientIds))
                //         .SendAsync("ReceiveConversation");
                // }

                var notificationModel = _mapper.Map<NotificationModel>(notification);
                await _hubContext.Clients
                    .Clients(_connections.GetConnections(notification.AccountId))
                    .SendAsync("ReceiveNotification", notificationModel);
                // await _hubContext.Clients
                //     .Clients(_connections.GetConnections(notification.AccountId)).SendAsync("ReceiveMessage");

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Send notication successfully"
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status500InternalServerError,
                Message = "Cannot create message"
            };
        }
    }
}
