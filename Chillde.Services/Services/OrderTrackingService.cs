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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

namespace Chillde.Services.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimService _claimService;
        private readonly ICloudinaryHelper _cloudinaryHelper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISystemConfigService _systemConfigService;
        private readonly IOrderReminderService _orderReminderService;

        public OrderTrackingService(IUnitOfWork unitOfWork, IClaimService claimService, ICloudinaryHelper cloudinaryHelper, IServiceProvider serviceProvider, ISystemConfigService systemConfigService, IOrderReminderService orderReminderService)
        {
            _unitOfWork = unitOfWork;
            _claimService = claimService;
            _cloudinaryHelper = cloudinaryHelper;
            _serviceProvider = serviceProvider;
            _systemConfigService = systemConfigService;
            _orderReminderService = orderReminderService;
        }

        public async Task<ResponseModel> ChangeAccepted(Guid orderTrackingId, bool isAccept)
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


            if (isAccept)
            {
                if (orderTracking.Type == OrderTrackingType.Sketch)
                {
                    orderTracking.IsAccepted = true;
                    orderTracking.Order.Stage = OrderStage.DeliveryInProcess;
                    orderTracking.Order.ReminderSent = false;
                    orderTracking.Order.DeadlineMissed = false;
                    _unitOfWork.OrderTrackingRepository.Update(orderTracking);
                    await _unitOfWork.SaveChangeAsync();
                    _orderReminderService.UpdateSchedule();
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Sketch tracking accepted. Order stage updated to Delivery In Process."
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Cannot accept tracking for this type."
                };
            }
            else if (!isAccept)
            {
                if (orderTracking.Type == OrderTrackingType.Sketch)
                {
                    orderTracking.IsAccepted = false;
                    orderTracking.Order.CurrentSketchRevision -= 1;
                    if (orderTracking.Order.CurrentSketchRevision < 0)
                    {
                        orderTracking.Order.CurrentSketchRevision = 0;
                    }
                    _unitOfWork.OrderTrackingRepository.Update(orderTracking);
                    await _unitOfWork.SaveChangeAsync();
                   
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status200OK,
                        Message = "Sketch tracking rejected. Current sketch revision updated."
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Cannot reject tracking for this type."
                };
            }

            return new ResponseModel
            {
                Code = StatusCodes.Status400BadRequest,
                Message = "Invalid type provided. Must be 'accepted' or 'rejected'."
            };
        }
   
    }
}
