using System;
using System.Threading.Tasks;

namespace WebAPI.Services.RabbitMQService
{
    public interface IBus
    {
        Task ReceiveAsync<T>(string queue, Action<T> onMessage);
    }
}