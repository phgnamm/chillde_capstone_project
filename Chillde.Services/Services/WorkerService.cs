using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Chillde.Repositories.Entities;
using Microsoft.Extensions.Configuration;
using Chillde.Services.Interfaces;
using Chillde.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Chillde.Repositories.Models.UserActivityLogModels;

namespace Chillde.Services.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkerService> _logger;
        private readonly ConcurrentQueue<UserActivityLogAddModel> _logQueue;
        private readonly PeriodicTimer _timer;

        public WorkerService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<WorkerService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _logQueue = new ConcurrentQueue<UserActivityLogAddModel>();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(300));

            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQ:HostName"]! };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "user_activity_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation("WorkerService initialized and connected to RabbitMQ.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkerService is starting.");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var log = JsonSerializer.Deserialize<UserActivityLogAddModel>(message);

                if (log != null)
                {
                    _logQueue.Enqueue(log);
                    _logger.LogInformation("Queued log for UserId {UserId}: {ActivityType}", log.UserId, log.ActivityType);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: "user_activity_queue", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _timer.WaitForNextTickAsync(stoppingToken);
                await ProcessLogs();
            }
        }

        private async Task ProcessLogs()
        {
            if (_logQueue.IsEmpty)
            {
                _logger.LogInformation("No logs to process.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var openAiService = scope.ServiceProvider.GetRequiredService<IOpenAiService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var logs = new List<UserActivityLogAddModel>();
            while (_logQueue.TryDequeue(out var log))
            {
                logs.Add(log);
            }
            _logger.LogInformation("Processing {Count} logs...", logs.Count);

            var groupedLogs = logs.GroupBy(log => log.UserId);

            foreach (var entry in groupedLogs)
            {
                var userId = entry.Key;
                var userLogs = entry.ToList();
                var userLogList = await unitOfWork.UserActivityLogRepository
                    .GetAllAsync(log => log.UserId == userId && log.Timestamp >= DateTime.UtcNow.AddSeconds(-1800));

                var recentLog = userLogList.Data.OrderByDescending(log => log.Timestamp).FirstOrDefault();

                var groupedByActivityType = userLogs
                    .GroupBy(log => log.ActivityType)
                    .Select(group => $"{group.Key}: {string.Join(" | ", group.Select(log => log.ActivityDetails))}")
                    .ToList();

                var combinedActivityDetails = string.Join("\n", groupedByActivityType);

                if (recentLog != null)
                {
                    if (!recentLog.ActivityDetails.Contains(combinedActivityDetails))
                    {
                        var newActivityDetails = $"{recentLog.ActivityDetails}\n{combinedActivityDetails}";
                        if (newActivityDetails.Length > 4000) 
                        {
                            newActivityDetails = newActivityDetails.Substring(newActivityDetails.Length - 1800);
                        }

                        recentLog.ActivityDetails = newActivityDetails.Trim();
                        recentLog.Timestamp = DateTime.UtcNow;

                        recentLog.EmbeddingVector = await openAiService.GetEmbeddingAsync(new List<string> { newActivityDetails });

                        unitOfWork.UserActivityLogRepository.Update(recentLog);
                        _logger.LogInformation("Updated existing log for UserId {UserId}", userId);
                    }
                    else
                    {
                        _logger.LogInformation("No new activity to update for UserId {UserId}", userId);
                    }
                }
                else
                {
                    var embedding = await openAiService.GetEmbeddingAsync(new List<string> { combinedActivityDetails });

                    var userActivityLog = new UserActivityLog
                    {
                        UserId = userId,
                        ActivityType = "Grouped Activities",
                        ActivityDetails = combinedActivityDetails,
                        EmbeddingVector = embedding,
                        Timestamp = DateTime.UtcNow
                    };

                    await unitOfWork.UserActivityLogRepository.AddAsync(userActivityLog);
                    _logger.LogInformation("Saved new grouped log for UserId {UserId}", userId);
                }
            }

            await unitOfWork.SaveChangeAsync();
            _logger.LogInformation("All logs processed and saved.");
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WorkerService is stopping.");
            _channel.Close();
            _connection.Close();
            await base.StopAsync(cancellationToken);
        }
    }
}
