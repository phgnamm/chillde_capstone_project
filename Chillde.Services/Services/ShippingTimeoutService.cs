using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Services
{
    public class ShippingTimeoutService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShippingTimeoutService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromHours(48);
        public ShippingTimeoutService(IServiceProvider serviceProvider, ILogger<ShippingTimeoutService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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

                _logger.LogInformation($"Next check in {_checkInterval.TotalMinutes} minutes.");
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

                var orders = await unitOfWork.OrderRepository.GetAllAsync(
                    filter: o => o.Stage == OrderStage.Shipping
                              && o.Status == OrderStatus.Accepted
                              && !o.Shipments.Any()
                              && o.ModificationDate != null
                              && o.ModificationDate <= DateTime.UtcNow.Add(-_timeoutPeriod),
                    include: q => q.Include(o => o.CreatedBy)
                                   .Include(o => o.Package)
                                       .ThenInclude(p => p.Service)
                                           .ThenInclude(s => s.CreatedBy)
                );

                if (orders == null || !orders.Data.Any())
                {
                    _logger.LogInformation("No orders to process for shipping timeout.");
                    return;
                }

                foreach (var order in orders.Data)
                {
                    await CancelOrderAndNotifyAsync(order, unitOfWork, emailService);
                }

                await unitOfWork.SaveChangeAsync();
            }
        }

        private async Task CancelOrderAndNotifyAsync(Order order, IUnitOfWork unitOfWork, IEmailHelper emailService)
        {
            try
            {
                _logger.LogInformation($"Processing order {order.Code} for shipping timeout.");

                order.Status = OrderStatus.Cancelled;
                order.SystemCancelReason = SystemCancelReason.NotReponseDeadlineInTime;
                order.ModificationDate = DateTime.UtcNow;

                // Hoàn tiền và trừ điểm uy tín
                await CancelOrderAndRefund(order, unitOfWork);

                // Gửi email thông báo
                await SendCancellationEmail(order, emailService);

                unitOfWork.OrderRepository.Update(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing order {order.Code} for shipping timeout.");
            }
        }

        private async Task CancelOrderAndRefund(Order order, IUnitOfWork unitOfWork)
        {
            try
            {
                var customerAccount = await unitOfWork.AccountRepository.GetAsync(
                    (Guid)order.CreatedById,
                    include: _ => _.Include(a => a.Wallet)
                );

                var artisanAccount = order.Package.Service.CreatedBy.AccountRoles
                    .FirstOrDefault(ar => ar.Role.Name == Repositories.Enums.Role.Artisan.ToString());

                if (customerAccount?.Wallet == null || artisanAccount == null)
                {
                    _logger.LogError($"Refund failed for order {order.Code}: Invalid customer wallet or artisan account.");
                    return;
                }

                var refundTransaction = new Transaction
                {
                    Amount = order.TotalPrice,
                    Type = TransactionType.TransferIn,
                    Status = TransactionStatus.Completed,
                    WalletId = customerAccount.Wallet.Id,
                };

                customerAccount.Wallet.Balance += (decimal)order.TotalPrice;
                /*artisanAccount.TotalReputation = Math.Max(artisanAccount.TotalReputation - 15, 0);*/
               /* var reputationLog = new ReputationLog
                {
                    PointChange = -15,
                    Reason = $"Không tạo đơn vận chuyển cho đơn hàng {order.Code}",
                    OrderId = order.Id,
                };
                artisanAccount.Reputations.Add(reputationLog);*/
                order.Transactions.Add(refundTransaction);

                unitOfWork.AccountRoleRepository.Update(artisanAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to cancel order {order.Code}.");
                throw;
            }
        }

        private async Task SendCancellationEmail(Order order, IEmailHelper emailService)
        {
            try
            {
                if (order.CreatedBy?.Email == null || order.Package?.Service?.CreatedBy?.Email == null)
                {
                    _logger.LogWarning($"Invalid email addresses for order {order.Code}.");
                    return;
                }

                // Email cho khách hàng
                var customerEmailData = GenerateCancellationEmailForCustomer(order);
                await emailService.SendEmailAsync(
                    order.CreatedBy.Email,
                    customerEmailData.Subject,
                    customerEmailData.Content,
                    true);

                // Email cho nghệ nhân
                var artisanEmailData = GenerateCancellationEmailForArtisan(order);
                await emailService.SendEmailAsync(
                    order.Package.Service.CreatedBy.Email,
                    artisanEmailData.Subject,
                    artisanEmailData.Content,
                    true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending cancellation email for order {order.Code}.");
            }
        }

        private (string Subject, string Content) GenerateCancellationEmailForCustomer(Order order)
        {
            return (
                $"Đơn hàng #{order.Code} đã bị hủy do không tạo đơn vận chuyển",
                $@"
        <p>Chào bạn {EscapeHtml(order.CreatedBy.FirstName)} {EscapeHtml(order.CreatedBy.LastName)},</p>
        <p>Chúng tôi rất tiếc phải thông báo rằng đơn hàng #{order.Code} của bạn đã bị hủy do nghệ nhân không tạo đơn vận chuyển trong thời gian quy định.</p>
        <p><strong>Hoàn tiền</strong>: Số tiền {(decimal)order.TotalPrice:N0} VNĐ đã được hoàn lại vào ví của bạn.</p>
        <p>Nếu bạn có bất kỳ câu hỏi nào, xin vui lòng liên hệ với đội ngũ hỗ trợ.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
            );
        }

        private (string Subject, string Content) GenerateCancellationEmailForArtisan(Order order)
        {
            return (
                $"Đơn hàng #{order.Code} đã bị hủy do không tạo đơn vận chuyển",
                $@"
        <p>Chào bạn {EscapeHtml(order.Package.Service.CreatedBy.FirstName)} {EscapeHtml(order.Package.Service.CreatedBy.LastName)},</p>
        <p>Đơn hàng #{order.Code} đã bị hủy do bạn không tạo đơn vận chuyển trong thời gian quy định.</p>
        <p><strong>Điểm uy tín</strong>: Tài khoản của bạn đã bị trừ 15 điểm uy tín do vi phạm thời gian xử lý.</p>
        <p>Vui lòng đảm bảo tạo đơn vận chuyển đúng hạn cho các đơn hàng tiếp theo.</p>
        <p>Trân trọng,</p>
        <p>Đội ngũ Chillde</p>"
            );
        }

        private static string EscapeHtml(string input) =>
            System.Net.WebUtility.HtmlEncode(input?.Trim() ?? string.Empty);
    
    }
}
