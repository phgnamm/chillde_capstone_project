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
using Chillde.Repositories.Interfaces;
using Chillde.Services.Helpers;

public class DeliveryReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeliveryReminderService> _logger;
    private const int FIXED_DELAY_SECONDS = 30;

    public DeliveryReminderService(
        IServiceProvider serviceProvider,
        ILogger<DeliveryReminderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(FIXED_DELAY_SECONDS);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var provider = scope.ServiceProvider;
                var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
                var emailService = provider.GetRequiredService<IEmailHelper>();

                var orders = await unitOfWork.OrderRepository
                    .GetOrderToRemindDeadline();

                foreach (var order in orders)
                {
                    await ProcessOrderAsync(order, unitOfWork, emailService);
                }

                await unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing delivery reminders");
            }

            _logger.LogInformation($"Next check in {FIXED_DELAY_SECONDS} seconds");
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ProcessOrderAsync(
        Order order,
        IUnitOfWork unitOfWork,
        IEmailHelper emailService)
    {
        try
        {
            var now = DateTime.UtcNow;
            var startTime = order.StartTime.Value;
            var deliverySeconds = order.DeliveryTime * 86400;

            if (startTime == DateTime.MinValue || deliverySeconds <= 0)
            {
                _logger.LogWarning($"Invalid time configuration for order {order.Code}");
                return;
            }

            var deadline = startTime.AddSeconds((double)deliverySeconds);
            var reminderTime = startTime.AddSeconds((double)(deliverySeconds * 0.9));

            if (now >= reminderTime && !(bool)order.DeliveryReminderSent)
            {
                await HandleReminderPhase(order, unitOfWork, emailService, deadline);
            }
            else if (now >= deadline && !(bool)order.DeadlineMissed)
            {
                await HandleDeadlinePhase(order, unitOfWork, emailService, deadline);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing order {order.Code}");
        }
    }

    private async Task HandleReminderPhase(
        Order order,
        IUnitOfWork unitOfWork,
        IEmailHelper emailService,
        DateTime deadline)
    {
        if (order.DeliveryReminderSent == true) return;
        if (!ValidateOrderProperties(order)) return;

        await SendReminderEmail(order, emailService, deadline, false);
        order.DeliveryReminderSent = true;
        unitOfWork.OrderRepository.Update(order);
    }

    private async Task HandleDeadlinePhase(
        Order order,
        IUnitOfWork unitOfWork,
        IEmailHelper emailService,
        DateTime deadline)
    {
        if (order.DeadlineMissed == true) return;
        if (!ValidateOrderProperties(order)) return;

        await SendReminderEmail(order, emailService, deadline, true);
        await CancelOrderAndRefund(order, unitOfWork);
        order.DeadlineMissed = true;
        unitOfWork.OrderRepository.Update(order);
    }

    private bool ValidateOrderProperties(Order order)
    {
        var isValid = order.Package?.Service?.CreatedBy != null
            && order.CreatedBy?.Email != null
            && !string.IsNullOrWhiteSpace(order.CreatedBy.Email);

        if (!isValid)
        {
            _logger.LogWarning($"Invalid order properties for {order.Code}");
        }

        return isValid;
    }


    private async Task SendReminderEmail(
        Order order,
        IEmailHelper emailService,
        DateTime deadline,
        bool isDeadlineMissed)
    {
        var timeRemaining = deadline - DateTime.UtcNow;
        var timeDisplay = FormatTimeRemaining(timeRemaining);

        string subject = "";
        string content = "";

        if (isDeadlineMissed)
        {
            var artisanEmailData = GenerateDeadlineMissedEmailForArtisan(order, deadline, timeDisplay);
            var customerEmailData = GenerateDeadlineMissedEmailForCustomer(order, deadline, timeDisplay);

            subject = artisanEmailData.Subject;
            content = artisanEmailData.Content;
            await emailService.SendEmailAsync(
                order.Package.Service.CreatedBy.Email,
                subject,
                content,
                true);

            subject = customerEmailData.Subject;
            content = customerEmailData.Content;
            await emailService.SendEmailAsync(
                order.CreatedBy.Email,
                subject,
                content,
                true);
        }
        else
        {
            var reminderEmailData = GenerateReminderEmail(order, deadline, timeDisplay);
            subject = reminderEmailData.Subject;
            content = reminderEmailData.Content;

            await emailService.SendEmailAsync(
                order.Package.Service.CreatedBy.Email,
                subject,
                content,
                true);
        }
    }

    private (string Subject, string Content) GenerateReminderEmail(
    Order order,
    DateTime deadline,
    string timeDisplay)
    {
        return (
            $"Nhắc nhở: Còn {timeDisplay} để hoàn thành đơn #{order.Code}",
            $@"
        <p>Chào bạn {EscapeHtml(order.CreatedBy.FirstName)} {EscapeHtml(order.CreatedBy.LastName)},</p>
        <p><strong>Còn {timeDisplay} nữa</strong> để bạn gửi tệp hình ảnh sản phẩm cho đơn hàng #{order.Code}.</p>
        <p>Vui lòng tải lên tệp hình ảnh sản phẩm trước thời gian hết hạn vào <strong>{deadline.ToString("dd MMMM yyyy HH:mm")}</strong> để đảm bảo đơn hàng được xử lý kịp thời.</p>
        <p>Nếu bạn đã nộp bản thảo, hãy bỏ qua email này. Nếu chưa, xin vui lòng thực hiện càng sớm càng tốt.</p>
        <p>Cảm ơn bạn đã hợp tác!</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
        );
    }


    private (string Subject, string Content) GenerateDeadlineMissedEmailForArtisan(
    Order order,
    DateTime deadline,
    string timeDisplay)
    {
        return (
            $"⚠️ Cảnh báo: Đơn #{order.Code} đã quá hạn và bị hủy",
            $@"
        <p>Chào bạn {EscapeHtml(order.CreatedBy.FirstName)} {EscapeHtml(order.CreatedBy.LastName)},</p>
        <p>Rất tiếc, chúng tôi thông báo rằng thời gian gửi hình ảnh sản phẩm cho đơn hàng #{order.Code} đã hết hạn và đơn hàng của bạn đã bị hủy.</p>
        <p>Đơn hàng lẽ ra phải có hình ảnh sản phẩm được nộp trước <strong>{deadline.ToString("dd MMMM yyyy HH:mm")}</strong>, nhưng chúng tôi chưa nhận được bản thảo. Vì vậy, đơn hàng đã bị hủy và chúng tôi sẽ hoàn tiền đầy đủ cho bạn.</p>
        <p>**Hoàn tiền**: Số tiền bạn đã thanh toán cho đơn hàng sẽ được hoàn lại đầy đủ vào tài khoản của bạn trong thời gian sớm nhất.</p>
        <p>**Điểm uy tín nghệ nhân**: Do không tuân thủ thời gian nộp bản thảo, điểm uy tín của nghệ nhân đã bị trừ 15 điểm trong hệ thống.</p>
        <p>Chúng tôi hiểu rằng có thể xảy ra một số vấn đề ngoài ý muốn và rất mong bạn thông cảm.</p>
        <p>Nếu bạn muốn tiếp tục với đơn hàng mới, xin vui lòng liên hệ với chúng tôi.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
        );
    }

    private (string Subject, string Content) GenerateDeadlineMissedEmailForCustomer(
    Order order,
    DateTime deadline,
    string timeDisplay)
    {
        return (
            $"⚠️ Cảnh báo: Đơn #{order.Code} đã quá hạn và bị hủy",
            $@"
        <p>Chào bạn {EscapeHtml(order.Package.Service.CreatedBy.FirstName)} {EscapeHtml(order.Package.Service.CreatedBy.LastName)},</p>
        <p>Rất tiếc, chúng tôi thông báo rằng do nghệ nhân đã không gửi hình ảnh sản phẩm lên đúng hạn của đơn hàng #{order.Code} và đơn hàng của bạn đã bị hủy.</p>
        <p>Đơn hàng lẽ ra phải có hình ảnh sản phẩm được nộp trước <strong>{deadline.ToString("dd MMMM yyyy HH:mm")}</strong>, nhưng chúng tôi chưa nhận được bản thảo. Vì vậy, đơn hàng đã bị hủy và chúng tôi sẽ hoàn tiền đầy đủ cho bạn.</p>
        <p>**Hoàn tiền**: Số tiền bạn đã thanh toán cho đơn hàng sẽ được hoàn lại đầy đủ vào tài khoản của bạn trong thời gian sớm nhất.</p>
        <p>**Điểm uy tín nghệ nhân**: Do không tuân thủ thời gian nộp bản thảo, điểm uy tín của nghệ nhân đã bị trừ trong hệ thống.</p>
        <p>Chúng tôi hiểu rằng có thể xảy ra một số vấn đề ngoài ý muốn và rất mong bạn thông cảm.</p>
        <p>Nếu bạn muốn tiếp tục với đơn hàng mới, xin vui lòng liên hệ với chúng tôi.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
        );
    }


    private string FormatTimeRemaining(TimeSpan timeSpan)
    {
        var days = timeSpan.Days;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;

        var parts = new List<string>();

        if (days > 0)
            parts.Add($"🗓️ {days} ngày");
        if (hours > 0)
            parts.Add($"⏰ {hours} giờ");
        if (minutes > 0 || parts.Count == 0)
            parts.Add($"⏳ {minutes} phút");

        return string.Join(" ", parts);
    }
    private async Task CancelOrderAndRefund(Order order, IUnitOfWork unitOfWork)
    {
        try
        {
            var customerAccount = await unitOfWork.AccountRepository.GetAsync((Guid)order.CreatedById, include: _ => _.Include(_ => _.Wallet));


            var artisanAccount = order.Package.Service.CreatedBy.AccountRoles
                .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());

            if (customerAccount?.Wallet == null || artisanAccount == null)
            {
                _logger.LogError($"Refund failed for order {order.Code}");
                return;
            }

            var refundTransaction = new Transaction
            {
                Amount = order.TotalPrice,
                Type = TransactionType.TransferIn,
                Status = TransactionStatus.Completed,
                WalletId = customerAccount.Wallet.Id,
                Description = TransactionInformationHelper.TransferInInformation(order.Code, Chillde.Repositories.Enums.Role.Customer)
            };

            customerAccount.Wallet.Balance += (decimal)order.TotalPrice;
            artisanAccount.TotalReputation = Math.Max(artisanAccount.TotalReputation - 15, 0);
            var reputationLog = new ReputationLog
            {
                PointChange = -(int)15,
                Reason = $"Không đăng sản phẩm lên đúng thời gian của đơn hàng - {order.Code}",
                OrderId = order.Id,
            };
            artisanAccount.Reputations.Add(reputationLog);
            order.Transactions.Add(refundTransaction);
            order.Status = OrderStatus.Cancelled;
            order.Stage = OrderStage.Cancelled;
            order.SystemCancelReason = SystemCancelReason.NotPostDeliveryInTime;
            unitOfWork.AccountRoleRepository.Update(artisanAccount);
            unitOfWork.OrderRepository.Update(order);

            await unitOfWork.SaveChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to cancel order {order.Code}");
            throw;
        }
    }

    private static string EscapeHtml(string input) =>
        System.Net.WebUtility.HtmlEncode(input?.Trim() ?? string.Empty);
}
