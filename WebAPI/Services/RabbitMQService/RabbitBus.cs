using System;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Services.RabbitMQService;
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

        public async Task SendAsync<T>(string queue, T message)
        {
            await Task.Run(() =>
            {
                channel.QueueDeclare(queue, false, false, false);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = false;

                var output = JsonConvert.SerializeObject(message);
                channel.BasicPublish(string.Empty, queue, null, Encoding.UTF8.GetBytes(output));
            });
        }

        public async Task ReceiveAsync<T>(string queue, Action<T> onMessage)
        {
            channel.QueueDeclare(queue, false, false, false);
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (s, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var item = JsonConvert.DeserializeObject<T>(message); //retorna el elemento deserializado o null en el caso de un error de sintaxis
                onMessage(item);
                await Task.Yield();
            };
            channel.BasicConsume(queue, true, consumer);
            await Task.Yield();
        }
    }
}