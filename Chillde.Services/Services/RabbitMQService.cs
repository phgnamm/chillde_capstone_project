//using Chillde.Services.Interfaces;
//using RabbitMQ.Client;
//using System.Text;
//using System.Text.Json;

//namespace Chillde.Services.Services
//{
//    public class RabbitMQService : IRabbitMQService
//    {
//        private readonly IConnection _connection;

//        public RabbitMQService(IConnection connection)
//        {
//            _connection = connection;
//        }

//        public async Task SendMessageAsync<T>(string queueName, T message)
//        {
//            using var channel = _connection.CreateModel();
//            _ = channel.QueueDeclare(
//                queue: queueName,
//                durable: true,
//                exclusive: false,
//                autoDelete: false,
//                arguments: null);

//            var json = JsonSerializer.Serialize(message);
//            var body = Encoding.UTF8.GetBytes(json);

//            var properties = channel.CreateBasicProperties();
//            properties.Persistent = true;

//            await Task.Run(() =>
//            {
//                channel.BasicPublish(
//                    exchange: "",
//                    routingKey: queueName,
//                    basicProperties: properties,
//                    body: body);
//            });
//        }
//    }
//}
