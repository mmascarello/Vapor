using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using MqCommon;
using RabbitMQ.Client;

namespace VaporServer.MQHandler
{
    public class MQProducer
    {
        private readonly IModel channel;
         public  MQProducer()
         {
             var uri = new Uri(MqConstants.QueueUri);
            this.channel = new ConnectionFactory() { Uri = uri}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: MqConstants.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        
        private  Task<bool> SendLog(string message)
        {
            bool returnVal;
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                    routingKey: MqConstants.QueueName,
                    basicProperties: null,
                    body: body);
                returnVal = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnVal = false;
            }

            return Task.FromResult(returnVal);
        }
        
        public async Task SendLog(Log log)
        { 
            var stringLog = JsonSerializer.Serialize(log);
            var result = await SendLog(stringLog).ConfigureAwait(false);
            Console.WriteLine(result ? "Message {0} sent successfully" : "Could not send {0}", stringLog);
        }
    }
}