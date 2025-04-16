// Services/RabbitMQProducer.cs
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQTest.Models;
using RabbitMQ.Client.Events;
namespace RabbitMQTest.Services
{
    public class RabbitMQProducer
    {
        private readonly RabbitMQSettings _settings;

        public RabbitMQProducer(IOptions<RabbitMQSettings> options)
        {
            _settings = options.Value;
        }

        public void SendMessage(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _settings.QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: _settings.QueueName,
                                 basicProperties: null,
                                 body: body);
        }
    }

}
