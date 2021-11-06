using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogsServer.Endpoint
{
    public class LogConsumer
    {
        public async Task Start()
        {
            /*var factory = new ConnectionFactory();
            factory.Uri =
                new Uri("amqps://fhnocqil:3VamHErDywnXy607WYu3QD21i903fFTS@beaver.rmq.cloudamqp.com/fhnocqil");
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "log_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var log = JsonSerializer.Deserialize<Log>(message);
                    Console.WriteLine(" [x] Received log user [{0}], game [{1}], accion [{2}], response [{1}]", log.User, log.Game, log.Action, log.Response);
                };
                
                channel.BasicConsume(queue: "log_queue",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }*/
        }
    }
}