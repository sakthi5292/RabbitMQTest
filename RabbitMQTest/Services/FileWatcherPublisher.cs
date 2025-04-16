using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQTest.Models;

namespace RabbitMQTest.Services
{
    public class FileWatcherPublisher : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly string _filePath = "postgres sql.txt"; // Change this path as needed

        public FileWatcherPublisher(IOptions<RabbitMQSettings> options)
        {
            _settings = options.Value;
        }

        private void Connect()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Connect();

            long lastPosition = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!File.Exists(_filePath))
                {
                    await Task.Delay(1000);
                    continue;
                }

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Seek(lastPosition, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null && line !="")
                    {
                        var body = Encoding.UTF8.GetBytes(line);
                        _channel.BasicPublish("", _settings.QueueName, null, body);
                        Console.WriteLine($"Sent: {line}");
                    }
                }

                lastPosition = stream.Position;
                await Task.Delay(1000); // Check every second
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
