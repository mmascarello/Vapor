using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace VaporServer.MQHandler
{
    public class MQProducer
    {
        private readonly IModel channel;
         public  MQProducer()
         {
             var uri = new Uri("amqps://fhnocqil:3VamHErDywnXy607WYu3QD21i903fFTS@beaver.rmq.cloudamqp.com/fhnocqil");
            this.channel = new ConnectionFactory() { Uri = uri}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        
        public  Task<bool> SendLog(string message)
        {
            bool returnVal;
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                    routingKey: "log_queue",
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
    }
}