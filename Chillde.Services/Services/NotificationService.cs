using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Services.Hubs;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.MessageModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ResponseModel> PushNotification (NotificationAddModel notificationAddModel)
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
            //await _unitOfWork.NotificationRepository.AddAsync(notification);
            //if (await _unitOfWork.SaveChangeAsync() > 0)
            //{
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

                await _hubContext.Clients
                    .Clients(_connections.GetConnections(notification.AccountId))
                    .SendAsync("NotificationConversation", new { Message = "wtf"});
                await _hubContext.Clients
                    .Clients(_connections.GetConnections(notification.AccountId)).SendAsync("ReceiveMessage");

                return new ResponseModel
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Send notication successfully"
                };
            //}

            //return new ResponseModel
            //{
            //    Code = StatusCodes.Status500InternalServerError,
            //    Message = "Cannot create message"
            //};
        }
    }
}
