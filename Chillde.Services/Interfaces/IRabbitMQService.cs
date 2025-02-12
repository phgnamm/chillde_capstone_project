using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Interfaces
{
    public interface IRabbitMQService
    {
        Task SendMessageAsync<T>(string queueName, T message);
    }

}
