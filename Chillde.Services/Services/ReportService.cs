using AutoMapper;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITranslationService _translationService;
        private readonly IMapper _mapper;
        private readonly IBadWordFilterService _badWordFilterService;
        private readonly IPackageService _packageService;
        private readonly IRedisHelper _redisHelper;
        private readonly INotificationService _notificationService;

        public ReportService(IUnitOfWork unitOfWork,
            ITranslationService translationService,
            IMapper mapper,
            IBadWordFilterService badWordFilterService,
            IPackageService packageService,
            IRedisHelper redisHelper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
            _packageService = packageService;
            _redisHelper = redisHelper;
            _notificationService = notificationService;
        }

        public async Task<ResponseModel> Reject(Guid reportId, ReportRejectModel reportRejectModel)
        {
            try
            {
                var report = await _unitOfWork.ReportRepository.GetAsync(reportId);
                if (report == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Report not found."
                    };
                }

                if (string.IsNullOrWhiteSpace(reportRejectModel.Response))
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status422UnprocessableEntity,
                        Message = "Response is required."
                    };
                }

                report.Response = reportRejectModel.Response;
                report.Status = ReportStatus.Reject;
                _unitOfWork.ReportRepository.Update(report);

                var order = report.Order;
                order.Stage = OrderStage.Completed;
                order.Status = OrderStatus.Completed;
                _unitOfWork.OrderRepository.Update(order);

                var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.RejectReport).Result;
                if (notificationContent != null)
                {
                    var notificationAddModel = new NotificationAddModel
                    {
                        Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                        AccountId = (Guid)(order.Package.Service != null ? order.Package.Service.CreatedById : order.Package.Offer?.CreatedById)!,
                        NotificationContentId = notificationContent.Id,
                        SourceId = order.Id
                    };
                    await _notificationService.PushNotification(notificationAddModel);
                }

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    var reportModel = _mapper.Map<ReportModel>(report);

                    return new ResponseModel
                    {
                        Data = reportModel,
                        Code = StatusCodes.Status200OK,
                        Message = "Report is rejected"
                    };
                }

                return new ResponseModel
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Failed to reject."
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
        //public async Task<ResponseModel> Accept(Guid reportId)
        //{
        //    try
        //    {
        //        var report = await _unitOfWork.ReportRepository.GetAsync(reportId);
        //        if (report == null)
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status404NotFound,
        //                Message = "Report not found."
        //            };
        //        }

        //        if (string.IsNullOrWhiteSpace(reportRejectModel.Response))
        //        {
        //            return new ResponseModel
        //            {
        //                Code = StatusCodes.Status422UnprocessableEntity,
        //                Message = "Response is required."
        //            };
        //        }

        //        report.Response = reportRejectModel.Response;
        //        report.Status = ReportStatus.Reject;
        //        _unitOfWork.ReportRepository.Update(report);

        //        var order = report.Order;
        //        order.Stage = OrderStage.Completed;
        //        order.Status = OrderStatus.Completed;
        //        _unitOfWork.OrderRepository.Update(order);

        //        int result = await _unitOfWork.SaveChangeAsync();
        //        if (result > 0)
        //        {
        //            var reportModel = _mapper.Map<ReportModel>(report);

        //            return new ResponseModel
        //            {
        //                Data = reportModel,
        //                Code = StatusCodes.Status200OK,
        //                Message = "Report is rejected"
        //            };
        //        }

        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status400BadRequest,
        //            Message = "Failed to reject."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel
        //        {
        //            Code = StatusCodes.Status500InternalServerError,
        //            Message = ex.Message
        //        };
        //    }
        //}
    }
}
