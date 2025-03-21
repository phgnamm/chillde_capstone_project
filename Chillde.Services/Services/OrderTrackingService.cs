using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.OrderTrackingModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nest;

namespace Chillde.Services.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IClaimService _claimService;
        //private readonly ICloudinaryHelper _cloudinaryHelper;
        //private readonly ISystemConfigService _systemConfigService;

        //public OrderTrackingService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, ISystemConfigService systemConfigService)
        //{
        //    _unitOfWork = unitOfWork;
        //    _claimService = claimService;
        //    _cloudinaryHelper = cloudinaryHelper;
        //    _systemConfigService = systemConfigService;
        //}

        //public Task<ResponseModel> Accepted(Guid orderTrackingId)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<ResponseModel> Add(Guid orderId, OrderTrackingAddModel orderTrackingAddModel)
        //{
        //    var currentUserId = _claimService.GetCurrentUserId;
        //    if (!currentUserId.HasValue)
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status401Unauthorized,
        //            Message = "Unauthorized"
        //        };
        //    var order = await _unitOfWork.OrderRepository.GetAsync(orderId, include: _ => _.Include(_ => _.OrderTrackings));
        //    if (order == null)
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status404NotFound,
        //            Message = "Order cannot found."
        //        };
        //    var roles = await _unitOfWork.AccountRoleRepository.GetAllAsync(filter: _ => _.AccountId == currentUserId.Value, include: _ => _.Include(_ => _.Role));
        //    foreach(var role in roles.Data)
        //    {
        //        if(role.Role.Name == "Artisan")
        //        {
        //            var orderTracking = new OrderTracking
        //            {
        //                OrderId = orderId,
        //                Name = orderTrackingAddModel?.Name ?? "Unknown",
        //                Description = orderTrackingAddModel?.Description ?? "Unknown",
        //                Type = orderTrackingAddModel.Type ?? OrderTrackingType.Sketch,
        //                Stage = order.Stage,
        //                OrderTrackingAttachments = orderTrackingAddModel.OrderTrackingAttachmentAddModels?.Select(_ => new OrderTrackingAttachment
        //                {
        //                    AttachmentUrl = _.AttachmentUrl,
        //                    AttachmentAlt = _.AttachmentAlt
        //                }).ToList()
        //            };
        //            order.OrderTrackings.Add(orderTracking);
        //            // check neu type la sketch thi gui mail thong bao lien
        //            // neu type la none, khach hang phan hoi lau qua thi nghe nhan co quyen huy
        //        }
        //        else if(role.Role.Name == "Customer")
        //        {
        //            var orderTracking = new OrderTracking
        //            {
        //                OrderId = orderId,
        //                Name = orderTrackingAddModel?.Name ?? "Unknown",
        //                Description = orderTrackingAddModel?.Description ?? "Unknown",
        //                Type = OrderTrackingType.None,
        //                Stage = order.Stage,
        //                OrderTrackingAttachments = orderTrackingAddModel.OrderTrackingAttachmentAddModels?.Select(_ => new OrderTrackingAttachment
        //                {
        //                    AttachmentUrl = _.AttachmentUrl,
        //                    AttachmentAlt = _.AttachmentAlt
        //                }).ToList()
        //            };
        //            order.OrderTrackings.Add(orderTracking);
        //        }
        //    }
           
        //}

        //public Task<ResponseModel> Cancel(Guid orderTrackingId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ResponseModel> GetAll(Guid orderId, OrderStage orderStage)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ResponseModel> SendReminder(Guid orderTrackingId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
