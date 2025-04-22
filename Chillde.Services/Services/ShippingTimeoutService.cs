using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Kiểm tra mỗi 10 phút
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromHours(24); // 24 giờ

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
                                           .ThenInclude(a => a.AccountRoles)
                                           .ThenInclude(ar => ar.Role)
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

                    await CancelOrderAndNotifyAsync(order, unitOfWork, errorLogs);
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

        private async Task CancelOrderAndNotifyAsync(Order order, IUnitOfWork unitOfWork, List<string> errorLogs)
        {
            try
            {
                _logger.LogInformation($"Processing order {order.Code} for shipping timeout.");

                if (order.Stage != OrderStage.Shipping || order.Status != OrderStatus.Accepted || order.Shipments.Any())
                {
                    errorLogs.Add($"Order {order.Code} is not in Shipping, Accepted, or has shipments.");
                    _logger.LogWarning($"Order {order.Code} is not in Shipping, Accepted, or has shipments.");
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
                    order.SystemCancelReason = SystemCancelReason.NotReponseDeadlineInTime;
                    unitOfWork.OrderRepository.Update(order);

                    var refundTransaction = new Transaction
                    {
                        Amount = order.TotalPrice,
                        Type = TransactionType.TransferIn,
                        Status = TransactionStatus.Completed,
                        WalletId = customerAccount.Wallet.Id,
                        CreatedById = customerAccount.Id
                    };
                    customerAccount.Wallet.Balance += (decimal)order.TotalPrice;
                    order.Transactions.Add(refundTransaction);
                    unitOfWork.WalletRepository.Update(customerAccount.Wallet);
                    
                    await unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation($"Order {order.Code} cancelled successfully. Refunded and notified.");
                }
                catch (Exception ex)
                {
                    errorLogs.Add($"Error saving changes for order {order.Code}: {ex.Message}");
                    _logger.LogError(ex, $"Error saving changes for order {order.Code}.");
                    await unitOfWork.RollbackTransactionAsync();
                }
            }
            catch (Exception ex)
            {
                errorLogs.Add($"Error processing order {order.Code}: {ex.Message}");
                _logger.LogError(ex, $"Error processing order {order.Code}.");
            }
        }
    }
}
