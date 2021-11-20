using MqCommon;

namespace LogWebAPI.Services.RabbitMQService
{
    public class Queue
    {
        public static string ProcessingQueueName { get; } = MqConstants.QueueName; 
    }
}