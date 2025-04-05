using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Repositories.Enums;
using Chillde.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Chillde.Repositories.Entities;
using Chillde.Services.Interfaces;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Common;
using Nest;

namespace Chillde.Services.Services
{
    public class OrderReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderReminderService> _logger;
        private DateTime? _nextRunTime;

        public OrderReminderService(IServiceProvider serviceProvider, ILogger<OrderReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     try
            //     {
            //         using var scope = _serviceProvider.CreateScope();
            //         var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //         var emailService = scope.ServiceProvider.GetRequiredService<IEmailHelper>();
            //
            //         var orders = await dbContext.Orders
            //             .Where(o => o.Stage == OrderStage.DeliveryInProcess && o.StartTime.HasValue && o.DeliveryTime.HasValue)
            //              .Include(_ => _.Package)
            //             .ThenInclude(_ => _.Service)
            //             .ThenInclude(_ => _.CreatedBy)
            //             .Include(_ => _.CreatedBy)
            //             .ThenInclude(_ => _.Wallet)
            //             .Include(_ => _.CreatedBy)
            //             .ThenInclude(_ => _.AccountRoles)
            //             .Include(_ => _.OrderTrackings)
            //             .ToListAsync(stoppingToken);
            //                 
            //         var upcomingTimes = new List<DateTime>();
            //             var now = DateTime.UtcNow;
            //
            //         foreach (var order in orders)
            //         {
            //             var startTime = order.StartTime.Value.ToUniversalTime();
            //             var deliveryTime = TimeSpan.FromMinutes(order.DeliveryTime.Value * 1440);
            //             var deadline = startTime + deliveryTime;
            //             var reminderTime = startTime + TimeSpan.FromTicks((long)(deliveryTime.Ticks * 0.9));
            //
            //             if (now >= reminderTime && now < deadline && order.Stage == OrderStage.DeliveryInProcess)
            //             {
            //                 //var artisan = order?.Package?.Service?.CreatedBy;
            //                 //if (artisan == null)
            //                 //{
            //                 //    _logger.LogWarning($"Order {order.Code}: Cannot send email because CreatedBy is null.");
            //                 //    return;
            //                 //}
            //                 if ((bool)!order.ReminderSent) 
            //                 {
            //                     if (order.CreatedBy?.Email != null)
            //                     {
            //                         await SendReminderEmail(order, emailService, deadline, false);
            //                         order.ReminderSent = true; 
            //                         dbContext.Orders.Update(order);
            //                         await dbContext.SaveChangesAsync(stoppingToken);
            //                     }
            //                 }
            //             }
            //
            //             if (now >= deadline && order.Stage == OrderStage.DeliveryInProcess)
            //             {
            //                 //var artisan = order?.Package?.Service?.CreatedBy;
            //                 //if (artisan == null)
            //                 //{
            //                 //    _logger.LogWarning($"Order {order.Code}: Cannot send email because CreatedBy is null.");
            //                 //    return;
            //                 //}
            //                 if ((bool)!order.DeadlineMissed) 
            //                 {
            //                     if (order.CreatedBy?.Email != null)
            //                     {
            //                         await SendReminderEmail(order, emailService, deadline, true);
            //                     }
            //
            //                     await CancelOrderAndRefund(order, dbContext, stoppingToken);
            //                     order.DeadlineMissed = true; 
            //                     dbContext.Orders.Update(order);
            //                     await dbContext.SaveChangesAsync(stoppingToken);
            //                 }
            //             }
            //             else
            //             {
            //                 upcomingTimes.Add(reminderTime);
            //                 upcomingTimes.Add(deadline);
            //             }
            //         }
            //
            //         if (upcomingTimes.Any())
            //         {
            //             _nextRunTime = upcomingTimes.Min();
            //         }
            //         else
            //         {
            //             _nextRunTime = now.AddMinutes(30);
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Error in OrderReminderService");
            //     }
            //
            //     var delay = _nextRunTime.HasValue ? _nextRunTime.Value - DateTime.UtcNow : TimeSpan.FromMinutes(30);
            //     if (delay < TimeSpan.Zero) delay = TimeSpan.Zero; 
            //
            //
            //     _logger.LogInformation($"Next run scheduled at: {_nextRunTime}");
            //     await Task.Delay(delay, stoppingToken);
            // }
        }

        private async Task SendReminderEmail(Order order, IEmailHelper emailService, DateTime responseDeadline, bool isDeadlineMissed)
        {
            // var timeRemaining = responseDeadline - DateTime.UtcNow;
            // string timeRemainingDisplay;
            //
            // if (timeRemaining.TotalMinutes < 60)
            // {
            //     timeRemainingDisplay = $"{timeRemaining.TotalMinutes:F0} minutes";
            // }
            // else if (timeRemaining.TotalHours < 24)
            // {
            //     timeRemainingDisplay = $"{timeRemaining.Hours} hours {timeRemaining.Minutes} minutes";
            // }
            // else
            // {
            //     timeRemainingDisplay = $"{timeRemaining.Days} days {timeRemaining.Hours} hours {timeRemaining.Minutes} minutes";
            // }
            //
            //
            // if (!isDeadlineMissed)
            // {
            //     await emailService.SendEmailAsync(
            //         order.CreatedBy.Email,
            //         $"Reminder: {timeRemainingDisplay} Left to Post Delivery",
            //                 $@"
            //         <p>Dear {order.CreatedBy.FirstName} {order.CreatedBy.LastName},</p>
            //         <p>This is a reminder that <strong>90% of your allocated response time</strong> for order <strong>#{order.Code}</strong> has been used.</p>
            //         <p><strong>Order Details:</strong></p>
            //         <ul>
            //             <li><strong>Service:</strong> {order.Package.Service.Name}</li>
            //             <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
            //             <li><strong>Response Deadline:</strong> {responseDeadline:yyyy-MM-dd HH:mm} UTC</li>
            //         </ul>
            //         <p>You now have <strong>{timeRemainingDisplay}</strong> left to post the delivery for this order.</p>
            //         <p>Please ensure that the delivery is posted before the deadline to avoid penalties.</p>
            //         <p>Best regards,</p>
            //         <p><strong>From Chillde</strong></p>",
            //                 true
            //     );
            // }
            // else
            // {
            //     await emailService.SendEmailAsync(
            //          order.CreatedBy.Email,
            //         $"Action Required: Missed Delivery Deadline for Order #{order.Code}",
            //         $@"
            //         <p>Dear {order.CreatedBy.FirstName} {order.CreatedBy.LastName},</p>
            //         <p>We regret to inform you that you have <strong>missed the delivery deadline</strong> for order <strong>#{order.Code}</strong>.</p>
            //         <p><strong>Order Details:</strong></p>
            //         <ul>
            //             <li><strong>Service:</strong> {order.Package.Service.Name}</li>
            //             <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
            //             <li><strong>Response Deadline:</strong> {responseDeadline:yyyy-MM-dd HH:mm} UTC</li>
            //         </ul>
            //         <p>As a result of this missed deadline, penalties may be applied according to our policy. Please contact support if you have any questions or need assistance.</p>
            //         <p>We strongly encourage you to ensure timely deliveries in the future to maintain a positive standing.</p>
            //         <p>Best regards,</p>
            //         <p><strong>From Chillde</strong></p>",
            //         true
            //     );
            // }
        }



        private async Task CancelOrderAndRefund(Order order, AppDbContext dbContext, CancellationToken stoppingToken)
        {
            // var accountCustomer = await dbContext.Accounts.Where(_ => _.CreatedById == order.CreatedById).Include(_ => _.Wallet).FirstOrDefaultAsync();
            // var accountRoleArtisan = order.Package.Service.CreatedBy?.AccountRoles
            //    .FirstOrDefault(_ => _.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());
            // var transaction = new Transaction
            // {
            //     Amount = order.TotalPrice,
            //     Type = TransactionType.TransferIn,
            //     CreatedById = null,
            //     Status = TransactionStatus.Completed,
            //     WalletId = accountCustomer.Wallet.Id
            // };
            // accountRoleArtisan.TotalReputation -= 5;
            // accountCustomer.Wallet.Balance += (decimal)order.TotalPrice;
            // order.Transactions.Add(transaction);
            // order.Status = OrderStatus.Cancelled;
            // order.Stage = OrderStage.Cancelled;
            // order.CancelOrderReason = CancelOrderReason.NotPostDeliveryInTime;
            // dbContext.Orders.Update(order);
            // dbContext.AccountRoles.Update(accountRoleArtisan);
            // await dbContext.SaveChangesAsync(stoppingToken);
        }
        private readonly object _lock = new object();

        public void TriggerImmediateCheck()
        {
            // lock (_lock)
            // {
            //     if (!_nextRunTime.HasValue || _nextRunTime.Value > DateTime.UtcNow)
            //     {
            //         _nextRunTime = DateTime.UtcNow;
            //     }
            // }
        }



    }

}
