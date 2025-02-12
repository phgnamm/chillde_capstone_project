using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Chillde.Repositories.Entities;
using Microsoft.Extensions.Configuration;
using Chillde.Services.Interfaces;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.UserActivityLogModels;
using Microsoft.Extensions.DependencyInjection;
using Chillde.Repositories;
using Microsoft.Extensions.Logging;

namespace Chillde.Services.Services
{
    public class WorkerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<WorkerService> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]!
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: "user_activity_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);

            _logger.LogInformation("WorkerService initialized and connected to RabbitMQ.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkerService is starting.");

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var openAiService = scope.ServiceProvider.GetRequiredService<IOpenAiService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var log = JsonSerializer.Deserialize<UserActivityLogAddModel>(message);

                    _logger.LogInformation("Message received from queue: {Message}", message);

                    if (log != null)
                    {
                        var embedding = await openAiService.GetEmbeddingAsync(new List<string> { log.ActivityType, log.ActivityDetails });
                        await SaveEmbeddingToDatabase(unitOfWork, log, embedding);
                        _channel.BasicAck(ea.DeliveryTag, false);

                        _logger.LogInformation("Message processed and acknowledged successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", ea.Body);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: "user_activity_queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("WorkerService is now consuming messages from the queue.");

            return Task.CompletedTask;
        }

        private async Task SaveEmbeddingToDatabase(IUnitOfWork unitOfWork, UserActivityLogAddModel log, float[] embedding)
        {
            var userActivityLog = new UserActivityLog
            {
                UserId = log.UserId,
                ActivityType = log.ActivityType,
                ActivityDetails = log.ActivityDetails,
                EmbeddingVector = embedding,
                Timestamp = log.Timestamp
            };

            await unitOfWork.UserActivityLogRepository.AddAsync(userActivityLog);

            _logger.LogInformation("User activity log saved to database for UserId: {UserId}", log.UserId);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WorkerService is stopping.");

            if (_channel != null)
            {
                _channel.Close();
                _channel.Dispose();
                _logger.LogInformation("RabbitMQ channel closed.");
            }
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _logger.LogInformation("RabbitMQ connection closed.");
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
