using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chillde.Services.Services
{
    public class SystemConfigActivationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SystemConfigActivationService> _logger;
        private DateTime? _nextRunTime;
        private const int FIXED_DELAY_SECONDS = 10;

        public SystemConfigActivationService(IServiceScopeFactory serviceScopeFactory, ILogger<SystemConfigActivationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = TimeSpan.FromSeconds(FIXED_DELAY_SECONDS);
            var now = DateOnly.FromDateTime(DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var newSystemConfigs = unitOfWork.SystemConfigRepository.GetAllAsync(filter: systemConfig => systemConfig.EffectiveFrom == now).Result.Data;

                        foreach (var newSystemConfig in newSystemConfigs)
                        {
                            var effectingSystemConfig = unitOfWork.SystemConfigRepository.GetAllAsync(filter:
                                systemConfig => systemConfig.FieldName == newSystemConfig.FieldName &&
                                                systemConfig.IsActive == true).Result.Data;

                            foreach (var systemConfig in effectingSystemConfig)
                            {
                                systemConfig.IsActive = false;
                            }

                                unitOfWork.SystemConfigRepository.UpdateRange(effectingSystemConfig);

                            newSystemConfig.IsActive = true;

                            unitOfWork.SystemConfigRepository.Update(newSystemConfig);
                        }

                        await unitOfWork.SaveChangeAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating vouchers.");
                }

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
