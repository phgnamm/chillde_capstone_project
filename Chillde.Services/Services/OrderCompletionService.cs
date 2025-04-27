using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chillde.Services.Helpers;

namespace Chillde.Services.Services
{
    public class OrderCompletionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCompletionService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60); 
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromHours(24); 

        public OrderCompletionService(IServiceProvider serviceProvider, ILogger<OrderCompletionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCompletionBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOrdersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing orders in OrderCompletionBackgroundService.");
                }
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("OrderCompletionBackgroundService is stopping.");
        }

        private async Task ProcessOrdersAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var timeoutThreshold = DateTime.UtcNow.Add(-_timeoutPeriod);

                var orders = await unitOfWork.OrderRepository.GetAllAsync(
                    filter: o => o.Stage == OrderStage.AwaitingClosure
                              && o.Status == OrderStatus.Accepted
                              && o.ModificationDate != null
                              && o.ModificationDate <= timeoutThreshold,
                    include: o => o.Include(o => o.Package)
                                  .ThenInclude(p => p.Service)
                                  .ThenInclude(s => s.CreatedBy)
                                  .Include(o => o.CreatedBy)
                );

                if (orders == null || !orders.Data.Any())
                {
                    _logger.LogInformation("No orders to process for completion.");
                    return;
                }

                foreach (var order in orders.Data)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancellation requested, stopping order processing.");
                        break;
                    }
                    try
                    {
                        _logger.LogInformation("Processing order {OrderId} for auto-completion.", order.Id);

                        if (order.Stage != OrderStage.AwaitingClosure || order.Status != OrderStatus.Accepted)
                        {
                            _logger.LogWarning($"Order {order.Id} is not in AwaitingClosure or Accepted. Skipping.");
                            continue;
                        }

                        var artisanAccount = await unitOfWork.AccountRepository.GetAsync(
                            (Guid)order.Package.CreatedById,
                            include: a => a.Include(a => a.Wallet).Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                        );
                        if (artisanAccount == null || artisanAccount.Wallet == null)
                        {
                            _logger.LogWarning($"Artisan account or wallet for order {order.Id} not found. Skipping.");
                            continue;
                        }

                        var customerAccount = await unitOfWork.AccountRepository.GetAsync(
                            (Guid)order.CreatedById,
                            include: a => a.Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                        );
                        if (customerAccount == null)
                        {
                            _logger.LogWarning($"Customer account for order {order.Id} not found. Skipping.");
                            continue;
                        }

                        var accountRoleArtisan = artisanAccount.AccountRoles
                            .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Artisan.ToString());
                        if (accountRoleArtisan == null)
                        {
                            _logger.LogWarning($"Artisan role not found for account {artisanAccount.Id} in order {order.Id}. Skipping.");
                            continue;
                        }

                        var accountRoleCustomer = customerAccount.AccountRoles
                            .FirstOrDefault(ar => ar.Role.Name == Chillde.Repositories.Enums.Role.Customer.ToString());
                        if (accountRoleCustomer == null)
                        {
                            _logger.LogWarning($"Customer role not found for account {customerAccount.Id} in order {order.Id}. Skipping.");
                            continue;
                        }

                        await unitOfWork.BeginTransactionAsync();
                        try
                        {
                            var wallet = artisanAccount.Wallet;
                            wallet.Balance += (decimal)order.ArtistRevenue;
                            unitOfWork.WalletRepository.Update(wallet);

                            order.Transactions.Add(new Transaction
                            {
                                WalletId = wallet.Id,
                                Amount = order.ArtistRevenue,
                                Type = TransactionType.TransferIn,
                                Status = TransactionStatus.Completed,
                                CreatedById = artisanAccount.CreatedById,
                                Description = TransactionInformationHelper.TransferInInformation(order.Code, Chillde.Repositories.Enums.Role.Artisan)

                            });

                            order.Stage = OrderStage.Completed;
                            order.Status = OrderStatus.Completed;
                            order.ModificationDate = DateTime.UtcNow;
                            unitOfWork.OrderRepository.Update(order);

                            if (accountRoleCustomer.TotalReputation < 100)
                            {
                                accountRoleCustomer.TotalReputation += 5;
                                unitOfWork.AccountRoleRepository.Update(accountRoleCustomer);

                                var customerReputationLog = new ReputationLog
                                {
                                    PointChange = +5,
                                    Reason = $"Đã hoàn thành đơn hàng {order.Code} với tư cách là khách hàng",
                                    OrderId = order.Id,
                                    AccountRoleId = accountRoleCustomer.Id,
                                    CreatedById = customerAccount.Id
                                };
                                await unitOfWork.ReputationLogRepository.AddAsync(customerReputationLog);
                                _logger.LogInformation($"Added 1 reputation point to customer {customerAccount.Id} for order {order.Id}.");
                            }

                            if (accountRoleArtisan.TotalReputation < 100)
                            {
                                accountRoleArtisan.TotalReputation += 5;
                                unitOfWork.AccountRoleRepository.Update(accountRoleArtisan);

                                var artisanReputationLog = new ReputationLog
                                {
                                    PointChange = +5,
                                    Reason = $"Đã hoàn thành đơn hàng {order.Code} với tư cách là nghệ nhân",
                                    OrderId = order.Id,
                                    AccountRoleId = accountRoleArtisan.Id,
                                    CreatedById = artisanAccount.Id
                                };
                                await unitOfWork.ReputationLogRepository.AddAsync(artisanReputationLog);
                                _logger.LogInformation($"Added 1 reputation point to artisan {artisanAccount.Id} for order {order.Id}.");
                            }

                            var saveResult = await unitOfWork.SaveChangeAsync();
                            if (saveResult <= 0)
                            {
                                _logger.LogError($"Failed to update order {order.Id} in database.");
                                await unitOfWork.RollbackTransactionAsync();
                                continue;
                            }

                            await unitOfWork.CommitTransactionAsync();
                            _logger.LogInformation($"Order {order.Id} updated to Completed. Wallet and reputation updated.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error saving changes for order {order.Id}.");
                            await unitOfWork.RollbackTransactionAsync();
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing order {order.Id}.");
                        continue;
                    }
                }
            }
        }
    }

}
