using System;
using System.Text;
using System.Text.Json;
using Domain;
using LogsServer.BusinessLogic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogsServer.Endpoint
{
    public class LogConsumer
    {
        private readonly IModel channel;
        private readonly LogLogic logLogic;

        public LogConsumer(LogLogic logLogic)
        {
            this.logLogic = logLogic;
            
            var uri = new Uri("amqps://fhnocqil:3VamHErDywnXy607WYu3QD21i903fFTS@beaver.rmq.cloudamqp.com/fhnocqil");
            this.channel = new ConnectionFactory() {Uri = uri}.CreateConnection().CreateModel();

            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        
        public void ReceiveLogs()
        {
            var log = new Log();
            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                log = JsonSerializer.Deserialize<Log>(message);
                logLogic.AddLog(log);
                Console.WriteLine(" Received log user [{0}], game [{1}], accion [{2}], response [{3}], date [{4}]",
                    log.User, log.Game, log.Action, log.Response, log.Date);
            };

            channel.BasicConsume(queue: "log_queue",
                autoAck: true,
                consumer: consumer);
        }
    }
}
