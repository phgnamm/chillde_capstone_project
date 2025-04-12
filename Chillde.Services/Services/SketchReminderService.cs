using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.SystemConfigModel;
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

        public SketchReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<SketchReminderService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
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

                        var responseDeadline = lastSketchTracking.CreationDate.AddSeconds(order.Package.ResponseTime * 60);
                        var totalResponseSeconds = order.Package.ResponseTime * 60; // responseTime đang là phút

                        var reminder50Time = lastSketchTracking.CreationDate.AddSeconds(totalResponseSeconds * 0.5);
                        var reminder80Time = lastSketchTracking.CreationDate.AddSeconds(totalResponseSeconds * 0.8);
                        if (now >= responseDeadline)
                        {
                            decimal penalty = 0m;
                            order.Status = OrderStatus.Cancelled;
                            order.Stage = OrderStage.Cancelled;
                            var autoCancelPercentagePenalty = await systemConfigurationService.Get(SystemConfigKey.AutoCancelPercentagePenalty);

                            if (autoCancelPercentagePenalty.Data is SystemConfigModel configPercentage
                                && decimal.TryParse(configPercentage.Value?.ToString(), out decimal autoCancelPercentagePenaltyValue))
                            {
                                penalty = (decimal)(order.TotalPrice * (autoCancelPercentagePenaltyValue / 100));
                            }

                            var autoCancelPointPenalty = await systemConfigurationService.Get(SystemConfigKey.AutoCancelPointPenalty);
                            decimal autoCancelPointPenaltyValue = 0m;
                            var customer = order.CreatedBy?.AccountRoles?.FirstOrDefault(_ => _.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());

                            if (autoCancelPointPenalty.Data is SystemConfigModel configPoint
                                && decimal.TryParse(configPoint.Value?.ToString(), out autoCancelPointPenaltyValue))
                            {

                                customer.TotalReputation -= (int)autoCancelPointPenaltyValue;
                                var reputationLog = new ReputationLog
                                {
                                    PointChange = -(int)autoCancelPointPenaltyValue,
                                    Reason = SystemCancelReason.NotReponseDeadlineInTime.ToString(),
                                    OrderId = order.Id,
                                };
                                customer.Reputations.Add(reputationLog);
                                unitOfWork.AccountRoleRepository.Update(customer);

                            }

                            var totalPriceAfterPenalty = order.TotalPrice - penalty;
                            order.CreatedBy.Wallet.Balance += (decimal)totalPriceAfterPenalty;

                            order.Transactions.Add(new Transaction
                            {
                                Amount = totalPriceAfterPenalty,
                                Type = TransactionType.TransferIn,
                                CreatedById = null,
                                Status = TransactionStatus.Completed,
                                WalletId = order.CreatedBy.Wallet.Id,
                            });
                            lastSketchTracking.IsDeadlineSent = true;
                            unitOfWork.OrderTrackingRepository.Update(lastSketchTracking);
                            order.ArtistRevenueAfterCancel = penalty;
                            order.SystemCancelReason = SystemCancelReason.NotReponseDeadlineInTime;
                            await emailService.SendEmailAsync(
                            order.CreatedBy.Email,
                            "Order Cancelled Due to No Response",
                            $@"
                               <p>Dear {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                               <p>We regret to inform you that your order <strong>#{order.Code}</strong> has been automatically cancelled because no response was received before the deadline.</p>
                               <p><strong>Order Details:</strong></p>
                               <ul>
                                   <li><strong>Service:</strong> {order.Package.Service.Name}</li>
                                   <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
                                   <li><strong>Response Deadline:</strong> {responseDeadline:yyyy-MM-dd HH:mm} UTC</li>
                               </ul>
                               <p>According to our cancellation policy, a penalty of <strong>${penalty}</strong> has been deducted. The remaining balance of <strong>${order.TotalPrice - penalty}</strong> has been refunded to your wallet.</p>
                               <p>If you have any concerns, please contact our support team.</p>
                               <p>Thank you for using our platform.</p>
                               <p>Best regards,</p>
                               <p><strong>From Chillde</strong></p>",
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
                                    $"Urgent: Only {timeRemainingDisplay80} Left to Respond",
                                    $@"
                                    <p>Dear {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                                    <p>This is a reminder that you have <strong>only {timeRemainingDisplay80}</strong> left to review the sketch for your order <strong>#{order.Code}</strong>.</p>
                                    <p><strong>Order Details:</strong></p>
                                    <ul>
                                        <li><strong>Service:</strong> {order.Package.Service.Name}</li>
                                        <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
                                        <li><strong>Response Deadline:</strong> {responseDeadline:yyyy-MM-dd HH:mm} UTC</li>
                                    </ul>
                                    <p>If you do not respond before the deadline, the order will be automatically cancelled, and a penalty may be applied.</p>
                                    <p>Best regards,</p>
                                    <p><strong>From Chillde</strong></p>",
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
                                    $"Reminder: {timeRemainingDisplay50} Left to Respond",
                                    $@"
                                    <p>Dear {order.CreatedBy.FirstName + " " + order.CreatedBy.LastName},</p>
                                    <p>We noticed that you have used up <strong>50% of your allocated response time</strong> for order <strong>#{order.Code}</strong>.</p>
                                    <p><strong>Order Details:</strong></p>
                                    <ul>
                                        <li><strong>Service:</strong> {order.Package.Service.Name}</li>
                                        <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
                                        <li><strong>Response Deadline:</strong> {responseDeadline:yyyy-MM-dd HH:mm} UTC</li>
                                    </ul>
                                    <p>You now have <strong>{timeRemainingDisplay50}</strong> left to review and approve the sketch.</p>
                                    <p>We encourage you to review the sketch as soon as possible to avoid potential cancellation.</p>
                                    <p>Best regards,</p>
                                    <p><strong>From Chillde</strong></p>",
                                    true
                                );
                                lastSketchTracking.IsReminder50Sent = true;
                                unitOfWork.OrderTrackingRepository.Update(lastSketchTracking);
                                await unitOfWork.SaveChangeAsync();
                            }

                        }

                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        private string FormatTimeDisplay(double minutes)
        {
            if (minutes < 60)
            {
                return $"{minutes} minutes";
            }
            else
            {
                return $"{minutes / 60:0.##} hours";
            }
        }


    }


}
