using System;
using WebAPI.Services.RabbitMQService;
using RabbitMQ.Client;

namespace WebAPI.Services.RabbitMQService
{
    public class RabbitHutch
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;

        public static IBus CreateBus(string uri)
        {
            _factory = new ConnectionFactory{ DispatchConsumersAsync = true };
            _factory.Uri = new Uri(uri);
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            return new RabbitBus(_channel);
        }

        public static IBus CreateBus(
            string hostName,
            ushort hostPort,
            string virtualHost,
            string username,
            string password)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = hostPort,
                VirtualHost = virtualHost,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true
            };

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            return new RabbitBus(_channel);
        }

    }
}