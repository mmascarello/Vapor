using System;
using RabbitMQ.Client;

namespace WebAPI.Services.RabbitMQService
{
    public class RabbitHutch
    {
        private static ConnectionFactory factory;
        private static IConnection connection;
        private static IModel channel;

        public static IBus CreateBus(string uri)
        {
            factory = new ConnectionFactory{ DispatchConsumersAsync = true };
            factory.Uri = new Uri(uri);
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            return new RabbitBus(channel);
        }

        public static IBus CreateBus(
            string hostName,
            ushort hostPort,
            string virtualHost,
            string username,
            string password)
        {
            factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = hostPort,
                VirtualHost = virtualHost,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            return new RabbitBus(channel);
        }

    }
}