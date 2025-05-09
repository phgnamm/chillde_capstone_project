using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chillde.Services.Services
{
    public class SketchReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SketchReminderService> _logger;
        private DateTime? _nextRunTime;
        private const int FIXED_DELAY_SECONDS = 10;

        public SketchReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<SketchReminderService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(FIXED_DELAY_SECONDS);

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine($"[SketchReminderService] Worker is running at {DateTime.UtcNow}");
                _logger.LogInformation("[SketchReminderService] Worker is running at {Time}", DateTime.UtcNow);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailHelper>();
                    var systemConfigurationService = scope.ServiceProvider.GetRequiredService<ISystemConfigService>();

                    var now = DateTime.UtcNow;
                    var orders = await unitOfWork.OrderRepository.GetSketchOrdersWithResponseTimeAsync();
                    _nextRunTime = null;

                    foreach (var order in orders)
                    {
                        var lastSketchTracking = order.OrderTrackings
                            .Where(t => t.Type == OrderTrackingType.Sketch && t.IsAccepted == null)
                            .OrderByDescending(t => t.CreationDate)
                            .FirstOrDefault();

                        if (lastSketchTracking == null) continue;
                        var penaltyPercentage = unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.AutoCancelPercentagePenalty).Result;

                        var responseDeadline = lastSketchTracking.CreationDate.AddSeconds(order.Package.ResponseTime * 60);
                        var totalResponseSeconds = order.Package.ResponseTime * 60; 
                        TimeZoneInfo timeZoneInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") 
                            : TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");     
                        DateTime localResponseDeadline = TimeZoneInfo.ConvertTimeFromUtc(responseDeadline, timeZoneInfo);
                        var reminder50Time = lastSketchTracking.CreationDate.AddSeconds(totalResponseSeconds * 0.5);
                        var reminder80Time = lastSketchTracking.CreationDate.AddSeconds(totalResponseSeconds * 0.8);
                        if (now >= responseDeadline)
                        {
                            decimal penalty = 0m;
                            order.Status = OrderStatus.Cancelled;
                            order.Stage = OrderStage.Cancelled;
                            
                                penalty = (decimal)(order.TotalPrice * (decimal.Parse(penaltyPercentage) / 100));
                            

                            var autoCancelPointPenalty =  unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.AutoCancelPointPenalty).Result;
                            var customer = order.CreatedBy?.AccountRoles?.FirstOrDefault(_ => _.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());

                                customer.TotalReputation -= (float.Parse(autoCancelPointPenalty));
                                var reputationLog = new ReputationLog
                                {
                                    PointChange = -(float.Parse(autoCancelPointPenalty)),
                                    Reason = $"Không phản hồi bản thảo đúng thời gian của đơn hàng - {order.Code}",
                                    OrderId = order.Id,
                                };
                                customer.Reputations.Add(reputationLog);
                                unitOfWork.AccountRoleRepository.Update(customer);


                            var totalPriceAfterPenalty = order.TotalPrice - penalty;
                            order.CreatedBy.Wallet.Balance += (decimal)totalPriceAfterPenalty;
                            order.Package.Service.CreatedBy.Wallet.Balance += (decimal)penalty;
                            order.Transactions.Add(new Transaction
                            {
                                Amount = totalPriceAfterPenalty,
                                Type = TransactionType.TransferIn,
                                CreatedById = null,
                                Status = TransactionStatus.Completed,
                                WalletId = order.CreatedBy.Wallet.Id,
                                Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Customer)

                            });
                            order.Transactions.Add(new Transaction
                            {
                                Amount = penalty,
                                Type = TransactionType.TransferIn,
                                CreatedById = null,
                                Status = TransactionStatus.Completed,
                                WalletId = order.Package.Service.CreatedBy.Wallet.Id,
                                Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Artisan)

                            });
                            lastSketchTracking.IsDeadlineSent = true;
                            unitOfWork.OrderTrackingRepository.Update(lastSketchTracking);
                            order.ArtistRevenueAfterCancel = penalty;
                            order.SystemCancelReason = SystemCancelReason.NotReponseDeadlineInTime;
                            await emailService.SendEmailAsync(
                                 order.CreatedBy.Email,
                                 "❌ Đơn hàng bị hủy do không phản hồi đúng hạn",
                                 $@"
                                <p>Xin chào {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                                <p>Đơn hàng <strong>#{order.Code}</strong> của bạn đã bị <strong>tự động hủy</strong> do không có phản hồi trước thời hạn quy định.</p>
                                <p><strong>Thông tin đơn hàng: </strong></p>
                                <ul>
                                    <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                                    <li><strong>Tổng giá trị:</strong> {order.TotalPrice} VNĐ</li>
                                    <li><strong>Hạn phản hồi:</strong> {localResponseDeadline:yyyy-MM-dd HH:mm} UTC</li>
                                </ul>
                                <p>Theo chính sách, bạn đã bị trừ <strong>{penalty} VNĐ</strong>. Số tiền còn lại <strong>{order.TotalPrice - penalty} VNĐ</strong> đã được hoàn vào ví của bạn.</p>
                                <p>Nếu bạn có thắc mắc, vui lòng liên hệ đội hỗ trợ của chúng tôi.</p>
                                <p>Trân trọng,</p>
                                <p><strong>Đội ngũ Chillde</strong></p>",
                                 true
                             );
                            unitOfWork.OrderRepository.Update(order);
                            await unitOfWork.SaveChangeAsync();


                        }
                        else
                        {
                            if (now >= reminder80Time && (bool)!lastSketchTracking.IsReminder80Sent && (bool)lastSketchTracking.IsReminder50Sent)
                            {
                                double timeRemainingMinutes80 = order.Package.ResponseTime * 0.2;
                                string timeRemainingDisplay80 = FormatTimeDisplay(timeRemainingMinutes80);

                                await emailService.SendEmailAsync(
                                order.CreatedBy.Email,
                                $"⚠️ Khẩn cấp: Chỉ còn {timeRemainingDisplay80} để phản hồi",
                                $@"
                                <p>Xin chào {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                                <p>Đây là lời nhắc: bạn chỉ còn <strong>{timeRemainingDisplay80}</strong> để xem xét bản phác thảo cho đơn hàng <strong>#{order.Code}</strong>.</p>
                                <p><strong>Thông tin đơn hàng:</strong></p>
                                <ul>
                                    <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                                    <li><strong>Tổng giá trị:</strong> {order.TotalPrice} VNĐ</li>
                                    <li><strong>Hạn phản hồi:</strong> {localResponseDeadline:yyyy - MM-dd HH:mm} UTC</li>
                                </ul>
                                <p>Nếu bạn không phản hồi trước thời hạn, đơn hàng sẽ bị hủy tự động và áp dụng phí phạt {float.Parse(penaltyPercentage)}% cho đơn hàng .</p>
                                <p>Trân trọng,</p>
                                <p><strong>Đội ngũ Chillde</strong></p>",
                                true
                            );

                                lastSketchTracking.IsReminder80Sent = true;
                                unitOfWork.OrderTrackingRepository.Update(lastSketchTracking);
                                await unitOfWork.SaveChangeAsync();
                            }
                            else if (now >= reminder50Time && (bool)!lastSketchTracking.IsReminder50Sent)
                            {
                                double timeRemainingMinutes50 = order.Package.ResponseTime * 0.5;
                                string timeRemainingDisplay50 = FormatTimeDisplay(timeRemainingMinutes50);

                              
                                await emailService.SendEmailAsync(
                                    order.CreatedBy.Email,
                                    $"⏰ Nhắc nhở: Còn {timeRemainingDisplay50} để phản hồi",
                                    $@"
                                    <p>Xin chào {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                                    <p>Bạn hiện còn <strong>{timeRemainingDisplay50}</strong> để xem xét và phê duyệt bản phác thảo.</p>
                                    <p><strong>Thông tin đơn hàng:</strong></p>
                                    <ul>
                                        <li><strong>Dịch vụ:</strong> {order.Package.Service.Name}</li>
                                        <li><strong>Tổng giá trị:</strong> {order.TotalPrice} VNĐ</li>
                                        <li><strong>Hạn phản hồi:</strong> {localResponseDeadline:yyyy-MM-dd HH:mm}</li>
                                    </ul>
                                    <p>Vui lòng phản hồi sớm để tránh đơn hàng bị hủy và áp dụng phí phạt {float.Parse(penaltyPercentage)}% cho đơn hàng.</p>
                                    <p>Trân trọng,</p>
                                    <p><strong>Đội ngũ Chillde</strong></p>",
                                    true
                                );

                                lastSketchTracking.IsReminder50Sent = true;
                                unitOfWork.OrderTrackingRepository.Update(lastSketchTracking);
                                await unitOfWork.SaveChangeAsync();
                            }

                        }

                    }
                }
                await Task.Delay(delay, stoppingToken);
            }
        }
        private string FormatTimeDisplay(double minutes)
        {
            var totalMinutes = (int)Math.Round(minutes);
            var hours = totalMinutes / 60;
            var remainingMinutes = totalMinutes % 60;

            if (hours > 0 && remainingMinutes > 0)
            {
                return $"⏰ {hours} giờ ⏳ {remainingMinutes} phút";
            }
            else if (hours > 0)
            {
                return $"⏰ {hours} giờ";
            }
            else
            {
                return $"⏳ {remainingMinutes} phút";
            }
        }


    }


}
