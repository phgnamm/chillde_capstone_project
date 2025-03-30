using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class OrderTrackingReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OrderTrackingReminderService> _logger;
        private DateTime? _nextRunTime;

        public OrderTrackingReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<OrderTrackingReminderService> logger)
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
                        var responseDeadline = order.OrderTrackings
                            .Where(_ => _.Type == OrderTrackingType.Sketch && _.IsAccepted == null)
                            .OrderByDescending(_ => _.CreationDate)
                            .Select(_ => _.CreationDate.AddHours(order.Package.ResponseTime))
                            .FirstOrDefault();

                        if (responseDeadline == default) continue;

                        var totalResponseTimeMinutes = order.Package.ResponseTime * 60;
                        var reminder50 = responseDeadline.AddMinutes(-totalResponseTimeMinutes * 0.5);
                        var reminder80 = responseDeadline.AddMinutes(-totalResponseTimeMinutes * 0.2);

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

                            if (autoCancelPointPenalty.Data is SystemConfigModel configPoint
                                && decimal.TryParse(configPoint.Value?.ToString(), out autoCancelPointPenaltyValue))
                            {
                                foreach (var role in order.CreatedBy.AccountRoles)
                                {
                                    role.TotalReputation -= (int)autoCancelPointPenaltyValue;
                                }
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

                            order.ArtistRevenueAfterCancel = penalty;
                            order.CancelOrderReason = CancelOrderReason.NotReponseDeadlineInTime;
                            unitOfWork.OrderRepository.Update(order);
                            await unitOfWork.SaveChangeAsync();


                            await emailService.SendEmailAsync(order.CreatedBy.Email, "Order Cancelled", "Your order was cancelled due to no response.", true);
                            _logger.LogInformation("Order {OrderId} cancelled due to no response.", order.Id);
                        }
                        else
                        {
                            if (now >= reminder80)
                            {
                                await emailService.SendEmailAsync(order.CreatedBy.Email, "Reminder: Order Response Needed", "You have less than 20% of your response time left.", true);
                            }
                            else if (now >= reminder50)
                            {
                                await emailService.SendEmailAsync(order.CreatedBy.Email, "Reminder: Order Response Needed", "You have used 50% of your response time.", true);
                            }

                        }

                        var upcomingReminder = new[] { reminder50, reminder80, responseDeadline }
                            .Where(t => t > now)
                            .OrderBy(t => t)
                            .FirstOrDefault();

                        if (upcomingReminder != default && (_nextRunTime == null || upcomingReminder < _nextRunTime))
                        {
                            _nextRunTime = upcomingReminder;
                        }
                    }
                }

                if (_nextRunTime.HasValue)
                {
                    var delay = _nextRunTime.Value - DateTime.UtcNow;
                    var safeDelay = TimeSpan.FromMilliseconds(Math.Max(0, delay.TotalMilliseconds));
                    _logger.LogInformation("Next check scheduled at {NextRunTime}", _nextRunTime.Value);
                    await Task.Delay(safeDelay, stoppingToken);
                }
            }
        }
    }


}
