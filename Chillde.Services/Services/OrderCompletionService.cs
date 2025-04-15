using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
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
    public  class OrderCompletionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCompletionService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60); // Kiểm tra mỗi 60 phút
        private readonly TimeSpan _timeoutPeriod = TimeSpan.FromHours(24); // 24 giờ

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
                var orders = await unitOfWork.OrderRepository.GetAllAsync(
                    filter: o => o.Stage == OrderStage.AwaitingClosure
                              && o.Status == OrderStatus.Accepted
                              && o.ModificationDate != null
                              && o.ModificationDate <= DateTime.UtcNow.Add(-_timeoutPeriod)
                );

                if (orders == null || !orders.Data.Any())
                {
                    _logger.LogInformation("No orders to process for completion.");
                    return;
                }

                foreach (var order in orders.Data)
                {
                    try
                    {
                        _logger.LogInformation("Processing order {OrderId} for auto-completion.", order.Id);

                        order.Stage = OrderStage.Completed;
                        order.Status = OrderStatus.Success;
                        unitOfWork.OrderRepository.Update(order);
                        var saveResult = await unitOfWork.SaveChangeAsync();

                        if (saveResult > 0)
                        {
                            _logger.LogInformation("Order {OrderId} updated to Completed and Success.", order.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to update order {OrderId} in database.", order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating order {OrderId}.", order.Id);
                    }
                }
            }
        }
    }
}
