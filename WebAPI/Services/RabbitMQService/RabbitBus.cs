using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebAPI.Services.RabbitMQService
{
    public class RabbitBus : IBus
    {
        private readonly IModel channel;

        internal RabbitBus(IModel channel)
        {
            this.channel = channel;
        }
        
        public async Task ReceiveAsync<T>(string queue, Action<T> onMessage)
        {
            channel.QueueDeclare(queue, false, false, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (s, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var item = JsonConvert.DeserializeObject<T>(message); 
                onMessage(item);
                await Task.Yield();
            };
            channel.BasicConsume(queue, true, consumer);
            await Task.Yield();
        }
    }
}