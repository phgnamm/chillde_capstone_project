using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Chillde.Repositories.Entities;
using Chillde.Services.Interfaces;
using Chillde.Repositories.Enums;
using Chillde.Repositories;

public interface IOrderReminderService
{
    void UpdateSchedule();
}

public class OrderReminderService : BackgroundService, IOrderReminderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderReminderService> _logger;
    private DateTime? _nextRunTime;
    private readonly object _lock = new object();

    public OrderReminderService(IServiceProvider serviceProvider, ILogger<OrderReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateScheduleInternal(stoppingToken);
            await WaitForNextRun(stoppingToken);
        }
    }

    public void UpdateSchedule()
    {
        _logger.LogInformation("UpdateSchedule called. Forcing schedule update...");
        Task.Run(async () => await UpdateScheduleInternal(CancellationToken.None));
    }

    private async Task UpdateScheduleInternal(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailHelper>();

            var orders = await dbContext.Orders
                .Where(o => o.Stage == OrderStage.DeliveryInProcess && o.StartTime.HasValue && o.DeliveryTime.HasValue && o.ReminderSent == false && o.DeadlineMissed == false)
                 .Include(o => o.Package)
                 .ThenInclude(p => p.Service)
                 .ThenInclude(s => s.CreatedBy)
                 .ThenInclude(c => c.AccountRoles)
                 .ThenInclude(r => r.Role)
                 .Include(o => o.CreatedBy)
                 .Include(o => o.OrderTrackings)
                .ToListAsync(stoppingToken);

            var upcomingTimes = new List<DateTime>();
            var now = DateTime.UtcNow;

            foreach (var order in orders)
            {
                var startTime = order.StartTime.Value;
                var deliveryTime = TimeSpan.FromMinutes(order.DeliveryTime.Value * 1440);
                var deadline = startTime + deliveryTime;
                var reminderTime = startTime + TimeSpan.FromTicks((long)(deliveryTime.Ticks * 0.2));

                if (now >= reminderTime && now < deadline && order.Stage == OrderStage.DeliveryInProcess)
                {
                    await SendReminderEmail(order, deadline, dbContext, stoppingToken, emailService);
                }

                if (now >= deadline && order.Stage == OrderStage.DeliveryInProcess)
                {
                    await HandleMissedDeadline(order, deadline, dbContext, stoppingToken, emailService);
                }
                else
                {
                    upcomingTimes.Add(reminderTime);
                    upcomingTimes.Add(deadline);
                }
            }

            lock (_lock)
            {
                _nextRunTime = upcomingTimes.Any() ? upcomingTimes.Min() : DateTime.UtcNow.AddMinutes(15);
                _logger.LogInformation($"Next run time calculated: {_nextRunTime}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateScheduleInternal");
        }
    }

    private async Task WaitForNextRun(CancellationToken stoppingToken)
    {
        try
        {
            DateTime nextRunTime;
            lock (_lock)
            {
                nextRunTime = _nextRunTime ?? DateTime.UtcNow.AddMinutes(15);
            }

            if (nextRunTime <= DateTime.UtcNow)
            {
                nextRunTime = DateTime.UtcNow.AddMinutes(15);
                lock (_lock)
                {
                    _nextRunTime = nextRunTime;
                }
            }

            var delay = nextRunTime - DateTime.UtcNow;
            _logger.LogInformation($"Waiting for {delay.TotalSeconds} seconds until next run...");
            await Task.Delay(delay, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Task.Delay was canceled.");
        }
    }

    private async Task SendReminderEmail(Order order, DateTime deadline, AppDbContext dbContext, CancellationToken stoppingToken, IEmailHelper emailHelper)
    {
        var artisan = order?.Package?.Service?.CreatedBy;
        if (artisan == null)
        {
            _logger.LogWarning($"Order {order.Code}: Cannot send email because CreatedBy is null.");
            return;
        }

        if ((bool)!order.ReminderSent)
        {
            var emailContent = GenerateReminderEmail(order, deadline);
            // Logic gửi email (giả định có một EmailService)
            _logger.LogInformation($"Sending reminder email for order {order.Code}...");
            emailHelper.SendEmailAsync(artisan.Email, "Reminder: Delivery Deadline Approaching", emailContent, true);

            order.ReminderSent = true;
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task HandleMissedDeadline(Order order, DateTime deadline, AppDbContext dbContext, CancellationToken stoppingToken, IEmailHelper emailHelper)
    {
        var artisan = order?.Package?.Service?.CreatedBy;
        if (artisan == null)
        {
            _logger.LogWarning($"Order {order.Code}: Cannot handle missed deadline because CreatedBy is null.");
            return;
        }

        if ((bool)!order.DeadlineMissed)
        {
            var emailContent = GenerateMissedDeadlineEmail(order, deadline);
            // Logic gửi email (giả định có một EmailService)
            _logger.LogInformation($"Sending missed deadline email for order {order.Code}...");
            emailHelper.SendEmailAsync(artisan.Email, "Missed Delivery Deadline", emailContent, true);

            order.DeadlineMissed = true;
            dbContext.Orders.Update(order);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    private string GetTimeRemaining(DateTime deadline)
    {
        var timeRemaining = deadline - DateTime.UtcNow;
        return timeRemaining.TotalMinutes < 60
            ? $"{timeRemaining.TotalMinutes:F0} minutes"
            : timeRemaining.TotalHours < 24
                ? $"{timeRemaining.Hours} hours {timeRemaining.Minutes} minutes"
                : $"{timeRemaining.Days} days {timeRemaining.Hours} hours {timeRemaining.Minutes} minutes";
    }

    private string GenerateReminderEmail(Order order, DateTime deadline) => $@"
        <p>Dear {order.CreatedBy.FirstName} {order.CreatedBy.LastName},</p>
        <p>This is a reminder that <strong>90% of your allocated response time</strong> for order <strong>#{order.Code}</strong> has been used.</p>
        <p><strong>Order Details:</strong></p>
        <ul>
            <li><strong>Service:</strong> {order.Package.Service.Name}</li>
            <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
            <li><strong>Response Deadline:</strong> {deadline:yyyy-MM-dd HH:mm} UTC</li>
        </ul>
        <p>You now have <strong>{GetTimeRemaining(deadline)}</strong> left to post the delivery for this order.</p>
        <p>Please ensure that the delivery is posted before the deadline to avoid penalties.</p>
        <p>Best regards,</p>
        <p><strong>From Chillde</strong></p>";

    private string GenerateMissedDeadlineEmail(Order order, DateTime deadline) => $@"
        <p>Dear {order.CreatedBy.FirstName} {order.CreatedBy.LastName},</p>
        <p>We regret to inform you that you have <strong>missed the delivery deadline</strong> for order <strong>#{order.Code}</strong>.</p>
        <p><strong>Order Details:</strong></p>
        <ul>
            <li><strong>Service:</strong> {order.Package.Service.Name}</li>
            <li><strong>Total Price:</strong> ${order.TotalPrice}</li>
            <li><strong>Response Deadline:</strong> {deadline:yyyy-MM-dd HH:mm} UTC</li>
        </ul>
        <p>As a result of this missed deadline, penalties may be applied according to our policy. Please contact support if you have any questions or need assistance.</p>
        <p>We strongly encourage you to ensure timely deliveries in the future to maintain a positive standing.</p>
        <p>Best regards,</p>
        <p><strong>From Chillde</strong></p>";
}
