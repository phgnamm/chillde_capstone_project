using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Services.Common;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        private readonly IClaimService _claimService;

        public ReportService(IUnitOfWork unitOfWork,
            ITranslationService translationService,
            IMapper mapper,
            IBadWordFilterService badWordFilterService,
            IPackageService packageService,
            IRedisHelper redisHelper,
            INotificationService notificationService,
            IClaimService claimService)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
            _packageService = packageService;
            _redisHelper = redisHelper;
            _notificationService = notificationService;
            _claimService = claimService;
        }
        public async Task<ResponseModel> GetAll(ReportFilterModel reportFilterModel)
        {

            Func<IQueryable<Report>, IOrderedQueryable<Report>> orderBy = query =>
            {
                switch (reportFilterModel.Order?.ToLower())
                {
                    case "recentdays":
                        return reportFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                    case "status":
                        return reportFilterModel.OrderByDescending
                            ? query.OrderBy(o => o.Status)
                            : query.OrderByDescending(o => o.Status);
                    default:
                        return reportFilterModel.OrderByDescending
                            ? query.OrderByDescending(o => o.CreationDate)
                            : query.OrderBy(o => o.CreationDate);
                }
            };

            Expression<Func<Report, bool>> filter = report =>
                (!reportFilterModel.OrderId.HasValue || report.OrderId == reportFilterModel.OrderId) &&
                (!reportFilterModel.IsDeleted.HasValue || report.IsDeleted == reportFilterModel.IsDeleted) &&
                 (string.IsNullOrEmpty(reportFilterModel.Search) || (
                                              report.Code.Contains(reportFilterModel.Search) 
                                          )) &&
                (!reportFilterModel.Status.HasValue || report.Status == reportFilterModel.Status);

            try
            {
                var reports = await _unitOfWork.ReportRepository.GetAllAsync(
                    filter: filter,
                    include: report => report.Include(o => o.ReportAttachments)
                                             .Include(_ => _.Order).ThenInclude(order => order.CreatedBy),
                    order: orderBy,
                    pageIndex: reportFilterModel.PageIndex,
                    pageSize: reportFilterModel.PageSize
                );

                var reportModels = reports.Data.Select(_ => new ReportModel
                {
                    Id = _.Id,
                    Description = _.Description,
                    OrderCode = _.Order.Code,
                    ReportCode = _.Code,
                    CustomerImage = _.Order.CreatedBy.Image,
                    CreationOrderDate = _.Order.CreationDate,
                    Response = _.Response,
                    Status = _.Status,
                    OrderId = _.OrderId,
                    ReportAttachments = _.ReportAttachments.Select(_ => new ReportAttachmentModel
                    {
                        AttachmentAlt = _.AttachmentAlt,
                        AttachmentUrl = _.AttachmentUrl
                    }).ToList(),
                    OrderCreationDate = _.Order.CreationDate,
                    CustomerName = $"{_.Order.CreatedBy.LastName} {_.Order.CreatedBy.FirstName}"
                }).ToList();

                //var pendingCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Pending && !_.IsDeleted).Result.Data.Count;
                //var acceptedCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Accepted && !_.IsDeleted).Result.Data.Count;
                //var rejectedCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Rejected && !_.IsDeleted).Result.Data.Count;

                //var reportModelWithCountStatus = new ReportModelWithCountStatus
                //{
                //    Pending = pendingCount,
                //    Accepted = acceptedCount,
                //    Rejected = rejectedCount,
                //    ReportModels = reportModels,
                //    Pagination = new PaginationInfo
                //    {
                //        CurrentPage = reportFilterModel.PageIndex,
                //        PageSize = reportFilterModel.PageSize,
                //        TotalPages = (int)Math.Ceiling(reports.TotalCount / (double)reportFilterModel.PageSize),
                //        TotalCount = reports.TotalCount
                //    }
                //};

                var result = new Pagination<ReportModel>(
                    reportModels,
                    reportFilterModel.PageIndex,
                    reportFilterModel.PageSize,
                    reports.TotalCount
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
        public async Task<ResponseModel> Reject(Guid reportId, ReportRejectOrAcceptModel reportRejectModel)
        {
            try
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

                var report = await _unitOfWork.ReportRepository.GetAsync(reportId, 
                    include: _ => _.Include(_ => _.Order).ThenInclude(_ => _.Package).ThenInclude(_ => _.Service)
                                   .Include(_ => _.Order).ThenInclude(_ => _.Package).ThenInclude(_ => _.Offer));
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
                report.Status = ReportStatus.Rejected;
                _unitOfWork.ReportRepository.Update(report);

                var order = report.Order;
                order.Stage = OrderStage.Completed;
                order.Status = OrderStatus.Completed;
                _unitOfWork.OrderRepository.Update(order);

                var artistianId = order.Package.Service != null ? order.Package.Service.CreatedById : order.Package.Offer?.CreatedById;

                var account = await _unitOfWork.AccountRepository.GetAsync((Guid)artistianId, include: _ => _.Include(_ => _.Wallet));
                var wallet = account?.Wallet;
                if (wallet == null)
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Wallet not found"
                    };

                wallet.Balance += (decimal)order.ArtistRevenue!;

                order.Transactions.Add(new Transaction
                {
                    WalletId = wallet.Id,
                    Amount = order.TotalPrice,
                    Type = TransactionType.TransferIn,
                    Status = TransactionStatus.Completed,
                    CreatedById = currentUserId
                });
                _unitOfWork.WalletRepository.Update(wallet);

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    var reportModel = _mapper.Map<ReportModel>(report);

                    var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_RejectReport).Result;
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

                    notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Artisan_RejectReport).Result;
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
        public async Task<ResponseModel> Accept(Guid reportId, ReportRejectOrAcceptModel reportAcceptModel)
        {
            try
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

                var report = await _unitOfWork.ReportRepository.GetAsync(reportId, 
                    include: report => report.Include(_ => _.Order).ThenInclude(_ => _.Package).ThenInclude(_ => _.Service)
                                             .Include(_ => _.Order).ThenInclude(_ => _.Package).ThenInclude(_ => _.Offer)
                    );
                if (report == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Report not found."
                    };
                }

                report.Response = reportAcceptModel.Response;
                report.Status = ReportStatus.Accepted;
                _unitOfWork.ReportRepository.Update(report);

                var order = report.Order;
                order.Stage = OrderStage.Completed;
                order.Status = OrderStatus.Refunded;
                _unitOfWork.OrderRepository.Update(order);

                //var account = await _unitOfWork.AccountRepository.GetAsync((Guid)order.CreatedById! , include: _ => _.Include(_ => _.Wallet));
                //var wallet = account?.Wallet;
                //if (wallet == null)
                //    return new ResponseModel
                //    {
                //        Code = StatusCodes.Status401Unauthorized,
                //        Message = "Wallet not found"
                //    };

                //wallet.Balance += (decimal)order.TotalPrice!;

                //order.Transactions.Add(new Transaction
                //{
                //    WalletId = wallet.Id,
                //    Amount = order.TotalPrice,
                //    Type = TransactionType.TransferOut,
                //    Status = TransactionStatus.Completed,
                //    CreatedById = currentUserId
                //});
                //_unitOfWork.WalletRepository.Update(wallet);

                int result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    var reportModel = _mapper.Map<ReportModel>(report);

                    var notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Customer_AcceptReport).Result;
                    if (notificationContent != null)
                    {
                        var notificationAddModel = new NotificationAddModel
                        {
                            Content = notificationContent.Content.Replace("[#orderCode]", order.Code),
                            AccountId = (Guid)order.CreatedById!,
                            NotificationContentId = notificationContent.Id,
                            SourceId = order.Id
                        };
                        await _notificationService.PushNotification(notificationAddModel);
                    }

                    notificationContent = _unitOfWork.NotificationContentRepository.GetByKeyAsync(NotificationCode.Artisan_AcceptReport).Result;
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

                    return new ResponseModel
                    {
                        Data = reportModel,
                        Code = StatusCodes.Status200OK,
                        Message = "Report is accepted."
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

        public async Task<ResponseModel> GetStatusCount()
        {
            var pendingCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Pending && !_.IsDeleted).Result.Data.Count;
            var acceptedCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Accepted && !_.IsDeleted).Result.Data.Count;
            var rejectedCount = _unitOfWork.ReportRepository.GetAllAsync(_ => _.Status == ReportStatus.Rejected && !_.IsDeleted).Result.Data.Count;

            var reportModelWithCountStatus = new ReportModelWithCountStatus
            {
                Pending = pendingCount,
                Accepted = acceptedCount,
                Rejected = rejectedCount,
                
            };

            return new ResponseModel
            {
                Message = "Get all reports successfully",
                Data = reportModelWithCountStatus,

            };
        }
    }
}
