using Chillde.Repositories.Common;
using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace Chillde.Services.Services
{
    public class ShippingTimeoutService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShippingTimeoutService> _logger;
        private readonly IEmailHelper _emailService;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromMinutes(20);

        public ShippingTimeoutService(
            IServiceProvider serviceProvider,
            ILogger<ShippingTimeoutService> logger,
            IEmailHelper emailService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ShippingTimeoutService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shipping timeouts.");
                }

                _logger.LogInformation($"Next check in {_checkInterval.TotalSeconds} seconds.");
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("ShippingTimeoutService is stopping.");
        }

        private async Task ProcessOrdersAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailHelper>();

                var now = DateTime.UtcNow;
                var timeoutThreshold = now.Add(-_timeoutPeriod);
                var reminderThreshold = now.Add(-_timeoutPeriod * 0.8);

                _logger.LogInformation($"Fetching orders: UTC now={now:yyyy-MM-dd HH:mm:ss}, Vietnam now={ToVietnamTime(now):dd MMMM yyyy HH:mm}, timeoutThreshold={timeoutThreshold}, reminderThreshold={reminderThreshold}");

                var orders = await unitOfWork.OrderRepository.GetAllAsync(
                    filter: o => o.Stage == OrderStage.Shipping
                              && o.Status == OrderStatus.Accepted
                              && !o.Shipments.Any()
                              && o.ModificationDate != null
                              && (o.ModificationDate <= timeoutThreshold || o.ModificationDate >= reminderThreshold),
                    include: q => q.Include(o => o.CreatedBy)
                                   .Include(o => o.Package)
                                       .ThenInclude(p => p.Service)
                                       .ThenInclude(s => s.CreatedBy)
                                       .ThenInclude(a => a.AccountRoles)
                                       .ThenInclude(ar => ar.Role)
                                   .Include(o => o.Package)
                                       .ThenInclude(p => p.Offer)
                );

                if (orders == null || !orders.Data.Any())
                {
                    _logger.LogInformation("No orders to process for shipping timeout.");
                    return;
                }

                var errorLogs = new List<string>();

                foreach (var order in orders.Data)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancellation requested, stopping order processing.");
                        break;
                    }

                    await ProcessOrderAsync(order, unitOfWork, emailService, errorLogs);
                }

                if (errorLogs.Any())
                {
                    _logger.LogWarning("Errors encountered during shipping timeout processing:\n{0}", string.Join("\n", errorLogs));
                }

                try
                {
                    await unitOfWork.SaveChangeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save changes for shipping timeout processing.");
                }
            }
        }

        private async Task ProcessOrderAsync(Order order, IUnitOfWork unitOfWork, IEmailHelper emailService, List<string> errorLogs)
        {
            try
            {
                _logger.LogInformation($"Processing order {order.Code} for shipping timeout.");

                var now = DateTime.UtcNow;
                var reminderThreshold = order.ModificationDate.Value.Add(_timeoutPeriod * 0.8);
                var timeoutThreshold = order.ModificationDate.Value.Add(_timeoutPeriod);

                _logger.LogInformation($"Order {order.Code}: UTC now={now:yyyy-MM-dd HH:mm:ss}, Vietnam now={ToVietnamTime(now)}, ModificationDate={order.ModificationDate}, reminderThreshold={reminderThreshold}, timeoutThreshold={timeoutThreshold}");

                if (order.Stage != OrderStage.Shipping || order.Status != OrderStatus.Accepted || order.Shipments.Any())
                {
                    errorLogs.Add($"Order {order.Code} is not in Shipping, Accepted, or has shipments.");
                    _logger.LogWarning($"Order {order.Code} is not in Shipping, Accepted, or has shipments.");
                    return;
                }

                if (now >= reminderThreshold && now < reminderThreshold.Add(TimeSpan.FromSeconds(10)))
                {
                    await SendReminderEmail(order, unitOfWork, emailService);
                }

                if (now >= timeoutThreshold)
                {
                    await CancelOrderAndNotifyAsync(order, unitOfWork, emailService, errorLogs);
                }
            }
            catch (Exception ex)
            {
                errorLogs.Add($"Error processing order {order.Code}: {ex.Message}");
                _logger.LogError(ex, $"Error processing order {order.Code}.");
            }
        }

        private async Task SendReminderEmail(Order order, IUnitOfWork unitOfWork, IEmailHelper emailService)
        {
            if (!ValidateOrderProperties(order))
            {
                return;
            }

            var deadline = ToVietnamTime(order.ModificationDate.Value.Add(_timeoutPeriod));
            var timeRemaining = deadline - ToVietnamTime(DateTime.UtcNow);
            var timeDisplay = FormatTimeRemaining(timeRemaining);

            var emailData = GenerateReminderEmail(order, deadline, timeDisplay);
            await emailService.SendEmailAsync(
                order.Package.Service.CreatedBy.Email,
                emailData.Subject,
                emailData.Content,
                true);

            _logger.LogInformation($"Sent reminder email for order {order.Code} to artisan {order.Package.Service.CreatedBy.Email}.");
        }

        private async Task CancelOrderAndNotifyAsync(Order order, IUnitOfWork unitOfWork, IEmailHelper emailService, List<string> errorLogs)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                if (!ValidateOrderProperties(order))
                {
                    errorLogs.Add($"Invalid order properties for {order.Code}");
                    return;
                }

                var customerAccount = await unitOfWork.AccountRepository.GetAsync(
                    (Guid)order.CreatedById,
                    include: a => a.Include(a => a.Wallet)
                );
                if (customerAccount == null || customerAccount.Wallet == null)
                {
                    errorLogs.Add($"Customer account or wallet for order {order.Code} not found.");
                    _logger.LogWarning($"Customer account or wallet for order {order.Code} not found.");
                    return;
                }

                var artisanAccount = await unitOfWork.AccountRepository.GetAsync(
                    (Guid)order.Package.Service.CreatedById,
                    include: a => a.Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                );
                if (artisanAccount == null)
                {
                    errorLogs.Add($"Artisan account for order {order.Code} not found.");
                    _logger.LogWarning($"Artisan account for order {order.Code} not found.");
                    return;
                }

                var accountRoleArtisan = artisanAccount.AccountRoles
                    .FirstOrDefault(ar => ar.Role.Name == Repositories.Enums.Role.Artisan.ToString());
                if (accountRoleArtisan == null)
                {
                    errorLogs.Add($"Artisan role not found for account {artisanAccount.Id} in order {order.Code}.");
                    _logger.LogWarning($"Artisan role not found for account {artisanAccount.Id} in order {order.Code}.");
                    return;
                }

                 await unitOfWork.BeginTransactionAsync();
                try
                {
                    order.Status = OrderStatus.Cancelled;
                    order.SystemCancelReason = SystemCancelReason.NotCreateShippingInTime;
                    order.ModificationDate = DateTime.UtcNow;
                    unitOfWork.OrderRepository.Update(order);

                    var refundTransaction = new Transaction
                    {
                        Amount = order.TotalPrice,
                        Type = TransactionType.TransferIn,
                        Status = TransactionStatus.Completed,
                        WalletId = customerAccount.Wallet.Id,
                        CreatedById = customerAccount.Id,
                        Description = TransactionInformationHelper.TransferInInformation(order.Code, Repositories.Enums.Role.Customer)
                    };
                    customerAccount.Wallet.Balance += (decimal)order.TotalPrice;
                    order.Transactions.Add(refundTransaction);
                    unitOfWork.WalletRepository.Update(customerAccount.Wallet);

                    await unitOfWork.CommitTransactionAsync();

                    await SendDeadlineMissedEmails(order, emailService);

                    _logger.LogInformation($"Order {order.Code} cancelled successfully. Refunded and emailed.");
                }
                catch (Exception ex)
                {
                    errorLogs.Add($"Error saving changes for order {order.Code}: {ex.Message}");
                    _logger.LogError(ex, $"Error saving changes for order {order.Code}.");
                    await unitOfWork.RollbackTransactionAsync();
                }
                finally
                {
                    transaction?.Dispose();
                }
            }
            catch (Exception ex)
            {
                errorLogs.Add($"Error processing order {order.Code}: {ex.Message}");
                _logger.LogError(ex, $"Error processing order {order.Code}.");
            }
        }

        private bool ValidateOrderProperties(Order order)
        {
            var isValid = order.Package?.Service?.CreatedBy != null
                       && order.CreatedBy?.Email != null
                       && !string.IsNullOrWhiteSpace(order.CreatedBy.Email)
                       && order.Package?.Service?.CreatedBy?.Email != null
                       && !string.IsNullOrWhiteSpace(order.Package.Service.CreatedBy.Email);

            if (!isValid)
            {
                _logger.LogWarning($"Invalid order properties for {order.Code}: Package={order.Package != null}, Service={order.Package?.Service != null}, ArtisanEmail={order.Package?.Service?.CreatedBy?.Email}, CustomerEmail={order.CreatedBy?.Email}");
            }

            return isValid;
        }

        private async Task SendDeadlineMissedEmails(Order order, IEmailHelper emailService)
        {
            var deadline = ToVietnamTime(order.ModificationDate.Value.Add(_timeoutPeriod));
            var timeDisplay = FormatTimeRemaining(TimeSpan.Zero);

            var customerEmailData = GenerateDeadlineMissedEmailForCustomer(order, deadline, timeDisplay);
            await emailService.SendEmailAsync(
                order.CreatedBy.Email,
                customerEmailData.Subject,
                customerEmailData.Content,
                true);

            var artisanEmailData = GenerateDeadlineMissedEmailForArtisan(order, deadline, timeDisplay);
            await emailService.SendEmailAsync(
                order.Package.Service.CreatedBy.Email,
                artisanEmailData.Subject,
                artisanEmailData.Content,
                true);

            _logger.LogInformation($"Sent deadline missed emails for order {order.Code} to customer {order.CreatedBy.Email} and artisan {order.Package.Service.CreatedBy.Email}.");
        }

        private (string Subject, string Content) GenerateReminderEmail(
            Order order,
            DateTime deadline,
            string timeDisplay)
        {
            var vietnameseDate = FormatVietnameseDate(deadline);
            return (
                $"Nhắc nhở: Còn {timeDisplay} để tạo vận đơn cho đơn #{order.Code}",
                $@"
        <p>Chào bạn {EscapeHtml(order.Package.Service.CreatedBy.FirstName)} {EscapeHtml(order.Package.Service.CreatedBy.LastName)},</p>
        <p><strong>Còn {timeDisplay}</strong> để bạn tạo vận đơn cho đơn hàng #{order.Code}.</p>
        <p>Vui lòng tạo vận đơn trước <strong>{vietnameseDate}</strong> để đảm bảo đơn hàng được xử lý kịp thời.</p>
        <p>Nếu bạn đã tạo vận đơn, hãy bỏ qua email này. Nếu chưa, xin vui lòng thực hiện ngay.</p>
        <p>Cảm ơn bạn đã hợp tác!</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
            );
        }

        private (string Subject, string Content) GenerateDeadlineMissedEmailForCustomer(
            Order order,
            DateTime deadline,
            string timeDisplay)
        {
            var vietnameseDate = FormatVietnameseDate(deadline);
            return (
                $"⚠️ Đơn #{order.Code} đã bị hủy do không tạo vận đơn",
                $@"
        <p>Chào bạn {EscapeHtml(order.CreatedBy.FirstName)} {EscapeHtml(order.CreatedBy.LastName)},</p>
        <p>Chúng tôi rất tiếc phải thông báo rằng đơn hàng #{order.Code} đã bị hủy vì nghệ nhân không tạo vận đơn trước thời hạn <strong>{vietnameseDate}</strong>.</p>
        <p><strong>Hoàn tiền</strong>: Số tiền bạn đã thanh toán ({order.TotalPrice:C}) sẽ được hoàn lại đầy đủ vào tài khoản của bạn trong thời gian sớm nhất.</p>
        <p>Chúng tôi xin lỗi vì sự bất tiện này và mong bạn thông cảm. Nếu bạn muốn đặt lại đơn hàng, vui lòng liên hệ với chúng tôi.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
            );
        }

        private (string Subject, string Content) GenerateDeadlineMissedEmailForArtisan(
            Order order,
            DateTime deadline,
            string timeDisplay)
        {
            var vietnameseDate = FormatVietnameseDate(deadline);
            return (
                $"⚠️ Đơn #{order.Code} đã bị hủy do không tạo vận đơn",
                $@"
        <p>Chào bạn {EscapeHtml(order.Package.Service.CreatedBy.FirstName)} {EscapeHtml(order.Package.Service.CreatedBy.LastName)},</p>
        <p>Chúng tôi rất tiếc phải thông báo rằng đơn hàng #{order.Code} đã bị hủy vì bạn không tạo vận đơn trước thời hạn <strong>{vietnameseDate}</strong>.</p>
        <p>Để tránh tình trạng này trong tương lai, vui lòng đảm bảo tạo vận đơn đúng hạn cho các đơn hàng ở trạng thái Shipping.</p>
        <p>Nếu bạn có thắc mắc hoặc cần hỗ trợ, vui lòng liên hệ với chúng tôi.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
            );
        }

        private string FormatTimeRemaining(TimeSpan timeSpan)
        {
            var days = timeSpan.Days;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            var parts = new List<string>();

            if (days > 0)
                parts.Add($"🗓️ {days} ngày");
            if (hours > 0)
                parts.Add($"⏰ {hours} giờ");
            if (minutes > 0)
                parts.Add($"⏳ {minutes} phút");
            if (seconds > 0 || parts.Count == 0)
                parts.Add($"⏳ {seconds} giây");

            return string.Join(" ", parts);
        }

        private static string EscapeHtml(string input) =>
            System.Net.WebUtility.HtmlEncode(input?.Trim() ?? string.Empty);

        private DateTime ToVietnamTime(DateTime utcTime)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, vietnamTimeZone);
        }

        private string FormatVietnameseDate(DateTime date)
        {
            var monthNames = new[] { "", "Tháng Một", "Tháng Hai", "Tháng Ba", "Tháng Tư", "Tháng Năm", "Tháng Sáu", "Tháng Bảy", "Tháng Tám", "Tháng Chín", "Tháng Mười", "Tháng Mười Một", "Tháng Mười Hai" };
            return $"{date:dd} {monthNames[date.Month]} {date:yyyy} {date:HH:mm}";
        }
    }
}