using AutoMapper;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.NotificationModels;
using Chillde.Repositories.Models.OrderTrackingModels;
using Chillde.Repositories.Models.ReportAttachmentModels;
using Chillde.Repositories.Models.ReportModels;
using Chillde.Repositories.Models.ShipmentModels;
using Chillde.Services.Common;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.ReportModels;
using Chillde.Services.Models.ResponseModels;
using Chillde.Services.Models.VoucherModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

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
        private readonly IOrderService _orderService;
        private readonly HttpClient _httpClient;
        private readonly IEmailHelper _emailHelper;

        public ReportService(IUnitOfWork unitOfWork,
            ITranslationService translationService,
            IMapper mapper,
            IBadWordFilterService badWordFilterService,
            IPackageService packageService,
            IRedisHelper redisHelper,
            INotificationService notificationService,
            IClaimService claimService,
            IHttpClientFactory httpClientFactory,
            IOrderService orderService, IEmailHelper emailHelper)
        {
            _unitOfWork = unitOfWork;
            _translationService = translationService;
            _mapper = mapper;
            _badWordFilterService = badWordFilterService;
            _packageService = packageService;
            _redisHelper = redisHelper;
            _notificationService = notificationService;
            _claimService = claimService;
            _httpClient = httpClientFactory.CreateClient("GhtkClient");
            _orderService = orderService;
            _emailHelper = emailHelper;
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
                                             .Include(_ => _.Order).ThenInclude(order => order.CreatedBy)
                                             .Include(_ => _.Order).ThenInclude(order => order.OrderTrackings).ThenInclude(orderTracking => orderTracking.OrderTrackingAttachments),
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
                    CreationDate = _.CreationDate,
                    Response = _.Response,
                    Status = _.Status,
                    OrderId = _.OrderId,
                    ReportAttachments = _.ReportAttachments.Select(_ => new ReportAttachmentModel
                    {
                        AttachmentAlt = _.AttachmentAlt,
                        AttachmentUrl = _.AttachmentUrl
                    }).ToList(),
                    Sketchs = _.Order.OrderTrackings.Where(orderTracking => orderTracking.Stage == OrderStage.SketchInProcess ||
                                                            orderTracking.Stage == OrderStage.ReviewSketch).Select(_ => new OrderTrackingModel
                                                            {
                                                                OrderTrackingAttachmentModels = _.OrderTrackingAttachments.Select(att => new OrderTrackingAttachmentModel
                                                                {
                                                                    Id = att.Id,
                                                                    AttachmentAlt = att.AttachmentAlt,
                                                                    AttachmentUrl = att.AttachmentUrl
                                                                }).ToList(),
                                                                IsAccepted = _.IsAccepted,

                                                            }).ToList(),
                    Deliveries = _.Order.OrderTrackings.Where(orderTracking => orderTracking.Stage == OrderStage.DeliveryInProcess ||
                                                            orderTracking.Stage == OrderStage.ReviewDelivery).Select(_ => new OrderTrackingModel
                                                            {
                                                                OrderTrackingAttachmentModels = _.OrderTrackingAttachments.Select(att => new OrderTrackingAttachmentModel
                                                                {
                                                                    Id = att.Id,
                                                                    AttachmentAlt = att.AttachmentAlt,
                                                                    AttachmentUrl = att.AttachmentUrl
                                                                }).ToList(),
                                                                IsAccepted = _.IsAccepted,
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
                    CreatedById = currentUserId,
                    Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Artisan)

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
                            AccountId = (Guid)(order.CreatedById),
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
                    //Gửi mail
                    TimeZoneInfo vnTimeZone = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                          ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") // Windows
                          : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");     // Linux

                    var localCreationDate = TimeZoneInfo.ConvertTimeFromUtc(report.CreationDate, vnTimeZone);

                    //Customer
                    await _emailHelper.SendEmailAsync(
                        order.CreatedBy.Email,
                        "❌ Kết quả xử lý báo cáo đơn hàng",
                        $@"
                        <p>Xin chào {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                        <p>Chúng tôi xin thông báo về kết quả xử lý báo cáo liên quan đến đơn hàng <strong>#{order.Code}</strong> mà bạn đã gửi vào ngày <strong>{localCreationDate:yyyy-MM-dd HH:mm}</strong>.</p>
                        <p>Sau quá trình kiểm tra và đánh giá cẩn trọng, <strong>chúng tôi xác định rằng báo cáo của bạn không đủ cơ sở để chấp nhận</strong>.</p>
                        <p>Do đó, đơn hàng đã được xử lý thành công và hoàn tất theo quy trình thông thường.</p>
                        <p><strong>Thông tin đơn hàng:</strong></p>
                        <ul>
                            <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                            <li><strong>Tổng giá trị đơn hàng:</strong> {order.TotalPrice} VNĐ</li>
                        </ul>
                        <p>Chúng tôi luôn trân trọng mọi phản hồi và mong tiếp tục nhận được sự tin tưởng từ bạn trong các đơn hàng tiếp theo.</p>
                        <p>Nếu bạn cần thêm hỗ trợ, vui lòng liên hệ đội ngũ chăm sóc khách hàng của chúng tôi.</p>
                        <p>Trân trọng,</p>
                        <p><strong>Đội ngũ Chillde</strong></p>
                        ",
                        true
                    );

                    //Artisan
                    await _emailHelper.SendEmailAsync(
                        order.Package.Service.CreatedBy.Email,
                        "✅ Đơn hàng được xác nhận hoàn tất",
                        $@"
                        <p>Xin chào {order.Package.Service.CreatedBy.FirstName + " " + order.Package.Service.CreatedBy.LastName},</p>
                        <p>Chúng tôi xin thông báo rằng đơn hàng <strong>#{order.Code}</strong> đã bị khách hàng báo cáo vào ngày <strong>{localCreationDate:yyyy-MM-dd HH:mm}</strong>, tuy nhiên sau khi kiểm tra kỹ lưỡng, <strong>chúng tôi xác định rằng đơn hàng đáp ứng đúng tiêu chuẩn</strong> như đã cam kết.</p>
                        <p><strong>Thông tin đơn hàng:</strong></p>
                        <ul>
                            <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                            <li><strong>Khách hàng:</strong> {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName}</li>
                            <li><strong>Giá trị đơn hàng:</strong> {order.TotalPrice} VNĐ</li>
                        </ul>
                        <p>Đơn hàng hiện đã được chuyển sang trạng thái hoàn tất. <strong>Số tiền {order.TotalPrice} VNĐ</strong> sẽ được chuyển vào ví Chillde của bạn.</p>
                        <p>Chúng tôi đánh giá cao chất lượng dịch vụ mà bạn đã cung cấp và mong bạn tiếp tục duy trì chất lượng trong những đơn hàng tiếp theo.</p>
                        <p>Nếu bạn cần thêm thông tin hoặc hỗ trợ, vui lòng liên hệ đội ngũ Chillde.</p>
                        <p>Trân trọng,</p>
                        <p><strong>Đội ngũ Chillde</strong></p>
                        ",
                        true
                    );


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
                                             .Include(_ => _.Order).ThenInclude(_ => _.Shipments).ThenInclude(_ => _.ProductShipments)
                                             .Include(_ => _.Order).ThenInclude(_ => _.CreatedBy)
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

                //tao shipment
                string partnerId = $"{order.Code}_Return_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                string partnerIdWithoutTime = partnerId.Substring(0, partnerId.IndexOf('_', partnerId.IndexOf('_') + 1));

                //lay id cua nghe nhan 
                var artisanId = order.Package.Service != null
                ? order.Package.Service.CreatedById
                : order.Package.Offer?.CreatedById;
                if (!artisanId.HasValue)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Cannot identify artisan for this order."
                    };
                }
                // lay dia chi nghe nhan 
                var artisanAddress = (await _unitOfWork.ShippingAddressRepository.GetAllAsync(
                    filter: sa => sa.CreatedById == artisanId.Value && sa.IsDefault && !sa.IsDeleted,
                    include: sa => sa.Include(x => x.CreatedBy)
                )).Data.FirstOrDefault();
                if (artisanAddress == null)
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Default shipping address for artisan not found."
                    };
                }
                var latestShipment = order.Shipments.OrderByDescending(s => s.CreationDate).FirstOrDefault();
                if (latestShipment == null || !latestShipment.ProductShipments.Any())
                {
                    return new ResponseModel
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "No product shipments found for this order."
                    };
                }
                var products = latestShipment.ProductShipments.Select(ps => new
                {
                    name = ps.Name,
                    weight = (double)ps.Weight,
                    quantity = ps.Quantity,
                }).ToList();
                //tao chuoi json 
                var jsonBody = JsonConvert.SerializeObject(new
                {
                    products,
                    order = new
                    {
                        id = partnerId,
                        pick_name = order.CreatedBy != null ? $"{order.CreatedBy.FirstName} {order.CreatedBy.LastName}" : "Customer",
                        pick_address = order.Address,
                        pick_province = order.ToProvince,
                        pick_district = order.ToDistrict,
                        pick_ward = order.ToWard,
                        pick_tel = order.Phone,
                        name = artisanAddress.FullName,
                        address = artisanAddress.AddressLine1 + "," + artisanAddress.AddressLine2,
                        province = artisanAddress.ProvinceName,
                        district = artisanAddress.DistrictName,
                        ward = artisanAddress.WardName,
                        tel = artisanAddress.PhoneNumber,
                        hamlet = "Khác",
                        email = artisanAddress.CreatedBy?.Email,
                        is_freeship = 1,
                        pick_money = 0,
                        note = $"Trả hàng cho đơn hàng {order.Code}",
                        value = (int)(order.TotalPrice ?? 0),
                        transport = "road",
                        pick_option = "cod",
                        deliver_option = "none",
                        tags = new string[] { "urgent", "fragile" }
                    }
                }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var url = "https://services-staging.ghtklab.com/services/shipment/order";
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
                {
                    Content = content
                };
                //Gửi mail
                TimeZoneInfo vnTimeZone = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                             ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") // Windows
                             : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");     // Linux

                var localCreationDate = TimeZoneInfo.ConvertTimeFromUtc(report.CreationDate, vnTimeZone);

                //Customer
                await _emailHelper.SendEmailAsync(
                     order.CreatedBy.Email,
                     "✅ Kết quả xử lý báo cáo đơn hàng",
                     $@"
                    <p>Xin chào {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                    <p>Chúng tôi xin thông báo về kết quả xử lý báo cáo liên quan đến đơn hàng <strong>#{order.Code}</strong> mà bạn đã gửi vào ngày <strong>{localCreationDate:yyyy-MM-dd HH:mm}</strong>.</p>
                    <p>Sau quá trình kiểm tra và đánh giá kỹ lưỡng, chúng tôi xác nhận rằng <strong>báo cáo của bạn là chính xác</strong>. Đơn hàng không đáp ứng đúng tiêu chuẩn như đã cam kết bởi nghệ nhân.</p>
                    <p><strong>Thông tin đơn hàng:</strong></p>
                    <ul>
                        <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                        <li><strong>Tổng giá trị đơn hàng:</strong> {order.TotalPrice} VNĐ</li>
                    </ul>
                    <p>Để hoàn tất quy trình hoàn trả, một đơn vị giao hàng sẽ đến địa chỉ của bạn trong thời gian sớm nhất để nhận lại sản phẩm và gửi trả về cho nghệ nhân.</p>
                    <p>Sau khi quá trình hoàn trả hoàn tất, <strong>toàn bộ số tiền {order.TotalPrice} VNĐ</strong> sẽ được hoàn lại vào ví Chillde của bạn.</p>
                    <p>Chúng tôi xin lỗi vì trải nghiệm chưa như mong đợi và luôn nỗ lực để nâng cao chất lượng dịch vụ. Nếu cần hỗ trợ thêm, bạn vui lòng liên hệ đội ngũ chăm sóc khách hàng của chúng tôi.</p>
                    <p>Trân trọng,</p>
                    <p><strong>Đội ngũ Chillde</strong></p>
                    ",
                     true
                 );

                //Artisan
                await _emailHelper.SendEmailAsync(
                  order.Package.Service.CreatedBy.Email,
                      "❌ Đơn hàng bị hoàn trả do báo cáo được xác nhận",
                      $@"
                    <p>Xin chào {order.Package.Service.CreatedBy.FirstName + " " + order.Package.Service.CreatedBy.LastName},</p>
                    <p>Chúng tôi xin thông báo về đơn hàng <strong>#{order.Code}</strong> do bạn thực hiện đã bị khách hàng báo cáo và <strong>báo cáo đã được xác nhận là hợp lệ</strong> sau quá trình kiểm tra kỹ lưỡng.</p>
                    <p><strong>Thông tin đơn hàng:</strong></p>
                    <ul>
                        <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                        <li><strong>Khách hàng:</strong> {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName}</li>
                        <li><strong>Giá trị đơn hàng:</strong> {order.TotalPrice} VNĐ</li>
                        <li><strong>Ngày báo cáo:</strong> {localCreationDate:yyyy-MM-dd HH:mm}</li>
                    </ul>
                    <p>Chúng tôi sẽ phối hợp với đơn vị vận chuyển để nhận lại sản phẩm từ khách hàng và chuyển trả lại cho bạn trong thời gian sớm nhất.</p>
                    <p>Rất mong bạn xem xét lại quy trình thực hiện dịch vụ để đảm bảo chất lượng tốt hơn trong tương lai. Mọi sai sót ảnh hưởng đến trải nghiệm của khách hàng đều được chúng tôi đánh giá nghiêm túc.</p>
                    <p>Nếu bạn có bất kỳ thắc mắc hay cần hỗ trợ, vui lòng liên hệ đội ngũ hỗ trợ của chúng tôi.</p>
                    <p>Trân trọng,</p>
                    <p><strong>Đội ngũ Chillde</strong></p>
                    ",
                     true
                 );


                //gui yeu cau shipment
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                var parsedJson = JsonConvert.DeserializeObject<ShipmentAddResponseModel>(responseContent);

                if (!response.IsSuccessStatusCode || parsedJson == null || parsedJson.Order == null)
                {
                    return new ResponseModel
                    {
                        Code = (int)response.StatusCode,
                        Message = parsedJson?.Success ?? "Failed to create return shipment. Invalid API response.",
                        Data = parsedJson
                    };
                }
                Shipment shipment = new()
                {
                    OrderId = order.Id,
                    TrackingId = parsedJson?.Order?.TrackingId.ToString() ?? string.Empty,
                    CurrentStatusId = (ShipmentStatus)(parsedJson.Order?.StatusId ?? 0),
                    PartnerId = parsedJson!.Order!.PartnerId,
                    Label = parsedJson.Order.Label,
                    Area = parsedJson.Order.Area,
                    Fee = parsedJson.Order.Fee != null ? decimal.Parse(parsedJson.Order.Fee) : 0,
                    InsuranceFee = parsedJson.Order.InsuranceFee != null ? decimal.Parse(parsedJson.Order.InsuranceFee) : 0,
                    EstimatedPickTime = parsedJson.Order.EstimatedPickTime,
                    EstimatedDeliverTime = parsedJson.Order.EstimatedDeliverTime,
                };
                if (products != null && products.Any())
                {
                    foreach (var product in products)
                    {
                        shipment.ProductShipments.Add(new ProductShipment
                        {
                            ShipmentId = shipment.Id,
                            Name = product.name ?? string.Empty,
                            Weight = (decimal)product.weight,
                            Quantity = product.quantity,
                            ProductCode = string.Empty
                        });
                    }
                }
                shipment.ShipmentStatusHistorys.Add(new ShipmentStatusHistory
                {
                    ShipmentId = shipment.Id,
                    StatusId = shipment.CurrentStatusId,
                });


                await _unitOfWork.ShipmentRepository.AddAsync(shipment);
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
