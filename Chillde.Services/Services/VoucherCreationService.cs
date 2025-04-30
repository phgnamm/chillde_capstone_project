using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.SystemConfigModel;
using Chillde.Services.Helpers;
using Chillde.Services.Interfaces;
using Chillde.Services.Models.VoucherModels;
using CloudinaryDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Chillde.Services.Services
{
    public class VoucherCreationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<VoucherCreationService> _logger;
        private DateTime? _nextRunTime;
        private const int FIXED_DELAY_SECONDS = 30;

        public VoucherCreationService(IServiceScopeFactory serviceScopeFactory, ILogger<VoucherCreationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(FIXED_DELAY_SECONDS);

            while (!stoppingToken.IsCancellationRequested)
            {
                //var now = DateTime.UtcNow;
                ////if (_nextRunTime == null || now >= _nextRunTime)
                ////{
                //_nextRunTime = GetNextRunTime();

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var minOrderToCreateVoucher = int.Parse(unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.MinOrdersForArtisan).Result!);
                        var discountValue = decimal.Parse(unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.DiscountValue).Result!);
                        var numberOfDateForUsingVoucher = int.Parse(unitOfWork.SystemConfigRepository.GetValueByKeyAsync(SystemConfigKey.NumberOfDateForUsingVoucher).Result!);

                        var accounts = unitOfWork.AccountRepository.GetAllAsync(
                            filter: account =>
                            account.IsDeleted == false &&
                            account.Status == AccountStatus.Active &&
                            account.AccountRoles.Any(_ => _.Role.Name == "Artisan")
                            ).Result.Data;

                        foreach (var account in accounts)
                        {
                            var successfulOrders = unitOfWork.OrderRepository
                                .GetAllAsync(order =>
                                             order.Status == OrderStatus.Completed &&
                                             order.Package.Service.CreatedById == account.Id &&
                                             order.DateTimeCreateVoucher == null)
                                .Result.Data;

                            if (successfulOrders.Count() >= minOrderToCreateVoucher)
                            {
                                int amountOfVouchers = successfulOrders.Count() / minOrderToCreateVoucher;
                                var voucher = new Voucher
                                {
                                    VoucherType = VoucherType.AdminToArtist,
                                    ReceiverId = account.Id,
                                    Code = GenerateCodeHelper.GenerateVoucherCode(),
                                    DiscountValue = discountValue,
                                    TotalQuantity = amountOfVouchers,
                                    StartTime = DateTime.UtcNow,
                                    ExpiredTime = DateTime.UtcNow.AddDays(numberOfDateForUsingVoucher),
                                    VoucherStatus = VoucherStatus.Pending
                                };
                                await unitOfWork.VoucherRepository.AddAsync(voucher);

                                foreach (var order in successfulOrders)
                                {
                                    order.DateTimeCreateVoucher = DateTime.UtcNow;
                                }
                                unitOfWork.OrderRepository.UpdateRange(successfulOrders);

                                await unitOfWork.SaveChangeAsync();
                                _logger.LogInformation($"Created voucher {voucher.Code} for account {account.Id}");

                            }
                        }
                        //var delay = _nextRunTime.Value - DateTime.UtcNow;
                        //if (delay.TotalMilliseconds < 0)
                        //{
                        //    _nextRunTime = GetNextRunTime();
                        //    delay = _nextRunTime.Value - DateTime.UtcNow;
                        //}

                        //await Task.Delay(delay, stoppingToken);

                        
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating vouchers.");
                }

                await Task.Delay(delay, stoppingToken);
                //}
            }
        }

        //private DateTime GetNextRunTime()
        //{
        //    var now = DateTime.UtcNow;
        //    var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        //    return new DateTime(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        //}
    }
}
