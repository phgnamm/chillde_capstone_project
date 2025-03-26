using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.OrderTrackingModels;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderTrackingModels;
using Chillde.Services.Models.ResponseModels;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;

namespace Chillde.Services.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly ISystemConfigService _systemConfigService;

        public OrderTrackingService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, ISystemConfigService systemConfigService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _systemConfigService = systemConfigService;
        }

        public async Task<ResponseModel> Add(Guid orderId, OrderTrackingAddModel orderTrackingAddModel)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: _ => _.Include(_ => _.OrderTrackings));
            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }

            var roles = await _unitOfWork.AccountRoleRepository.GetAllAsync(filter: _ => _.AccountId == currentUserId.Value, include: _ => _.Include(_ => _.Role));
            //var isArtisan = roles.Data.Any(role => role.Role.Name == "Artisan");
            //var isCustomer = roles.Data.Any(role => role.Role.Name == "Customer");

                    var uploadedAttachments = await UploadAttachments(orderTrackingAddModel.OrderTrackingAttachmentAddModels, order.Code, FolderAttachment.TRACKINGSKETCH);
                    var orderTracking = new OrderTracking
                    {
                        OrderId = orderId,
                        Name = orderTrackingAddModel?.Name ?? "Sketch",
                        Description = orderTrackingAddModel?.Description ?? "Sketch phase",
                        Type = OrderTrackingType.None,
                        Stage = OrderStage.ReviewSketch,
                        OrderTrackingAttachments = uploadedAttachments
                    };
      
                    await _unitOfWork.SaveChangeAsync();

                    //await _emailService.SendSketchNotification(order.CustomerEmail, order.Id, orderTracking.Name);

                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Sketch tracking added and notification sent."
                    };
                 
     
              
      
        }

        // Helper method to upload attachments
        private async Task<List<OrderTrackingAttachment>> UploadAttachments(IEnumerable<OrderTrackingAttachmentAddModel> attachments, string orderCode, string folderName)
        {
            var uploadedAttachments = new List<OrderTrackingAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var attachmentPath = await _cloudinaryHelper.UploadImageAsync(
                        attachment.AttachmentUrl,
                        orderCode,
                        folderName
                    );

                    uploadedAttachments.Add(new OrderTrackingAttachment
                    {
                        AttachmentUrl = attachmentPath,
                        AttachmentAlt = attachment.AttachmentAlt
                    });
                }
            }

            return uploadedAttachments;
        }


        public async Task<ResponseModel> Accepted(Guid orderTrackingId)
        {
            var orderTracking = await _unitOfWork.OrderTrackingRepository.GetAsync(orderTrackingId, include: _ => _.Include(_ => _.Order));
            if (orderTracking == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order tracking not found."
                };
            }

            if (orderTracking.Type == OrderTrackingType.Sketch)
            {
                orderTracking.IsAccepted = true;
                orderTracking.Order.Stage = OrderStage.DeliveryInProcess;
                await _unitOfWork.SaveChangeAsync();
                return new ResponseModel
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Sketch tracking accepted. Order stage updated to Making."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Cannot accept tracking for this type."
            };
        }

        public async Task<ResponseModel> Cancel(Guid orderId, Guid cancellationReasonId)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var account = await _unitOfWork.AccountRepository.GetAsync(
                currentUserId.Value, include: _ => _.Include(_ => _.Wallet));

            if (account?.Wallet == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Wallet not found."
                };
            }

            var order = await _unitOfWork.OrderRepository.GetAsync(
                orderId, include: _ => _.Include(_ => _.OrderTrackings)
                                        .Include(_ => _.Package.Service.CreatedBy.AccountRoles)
                                        .Include(_ => _.CreatedBy.AccountRoles));

            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }

            var cancellationReason = await _unitOfWork.CancellationReasonRepository.GetAsync(cancellationReasonId);

            if (cancellationReason == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Cancellation reason not found."
                };
            }

            bool appliesToCustomer = cancellationReason.RoleType.Equals(Chillde.Repositories.Enums.Role.Customer); 
            bool appliesToArtisan = cancellationReason.RoleType.Equals(Chillde.Repositories.Enums.Role.Artisan);

            var accountRoleArtisan = order.Package.Service.CreatedBy?.AccountRoles
                .FirstOrDefault(_ => appliesToArtisan && _.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());

            var accountRoleCustomer = order.CreatedBy?.AccountRoles
                .FirstOrDefault(_ => appliesToCustomer && _.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());

            if (appliesToArtisan && accountRoleArtisan != null)
            {
                accountRoleArtisan.TotalReputation -= cancellationReason.Value;
                _unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);
            }
            if (appliesToCustomer && accountRoleCustomer != null)
            {
                accountRoleCustomer.TotalReputation -= cancellationReason.Value;
                _unitOfWork.AccountRoleRepository.Update(accountRoleCustomer);
            }

            var transaction = new Transaction
            {
                Amount = order.TotalPrice,
                Type = TransactionType.TransferIn,
                CreatedById = currentUserId.Value,
                Status = TransactionStatus.Completed,
                WalletId = account.Wallet.Id
            };

            account.Wallet.Balance += (decimal)order.TotalPrice;
            order.Transactions.Add(transaction);
            order.Status = OrderStatus.Cancelled;
            order.Stage = OrderStage.Cancelled;
            order.CancellationReason = cancellationReason;

            _unitOfWork.OrderRepository.Update(order);

            var result = await _unitOfWork.SaveChangeAsync();

            return result > 0
                ? new ResponseModel { Message = "Cancel order successfully" }
                : new ResponseModel { Code = StatusCodes.Status400BadRequest, Message = "Cancel order unsuccessfully" };
        }



        public async Task<ResponseModel> GetAll(Guid orderId, OrderStage? orderStage)
        {
            var currentUserId = _claimService.GetCurrentUserId;
            if (!currentUserId.HasValue)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var order = await _unitOfWork.OrderRepository.GetAsync(
                orderId, include: _ => _.Include(_ => _.OrderTrackings)
                                         .ThenInclude(_ => _.CreatedBy)
                                         .ThenInclude(_ => _.AccountRoles)
                                         .Include(_ => _.OrderTrackings)
                                         .ThenInclude(_ => _.OrderTrackingAttachments));

            if (order == null)
            {
                return new ResponseModel
                {
                    Code = StatusCodes.Status404NotFound,
                    Message = "Order not found."
                };
            }

            var filteredTrackings = order.OrderTrackings
                .Where(t => !orderStage.HasValue || t.Stage == orderStage)
                .Select(t => new OrderTrackingModel
                {
                    Id = t.Id,
                    CreatedBy = $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}",
                    DeletedById = t.CreatedById,
                    CreatedRole = t.CreatedBy.AccountRoles.FirstOrDefault()?.Role?.Name ?? "Unknown",
                    CurrentSketchRevision = order.CurrentSketchRevision, 
                    Name = t.Name,
                    Description = t.Description,
                    IsAccepted = t.IsAccepted,
                    Stage = t.Stage,
                    Type = t.Type,
                    CreationDate = t.CreationDate,
                    OrderTrackingAttachmentModels = t.OrderTrackingAttachments?
                        .Select(a => new OrderTrackingAttachmentModel
                        {
                            Id = a.Id,
                            AttachmentUrl = a.AttachmentUrl,
                            AttachmentAlt = a.AttachmentAlt
                        }).ToList()
                })
                .ToList();

            return new ResponseModel
            {
                Data = filteredTrackings.Any() ? filteredTrackings : null,
                Message = "Order trackings retrieved successfully."
            };
        }


        public Task<ResponseModel> SendReminder(Guid orderTrackingId)
        {
            throw new NotImplementedException();
        }
    }
}
