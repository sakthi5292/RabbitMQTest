namespace RabbitMQSubscriber.Services
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Microsoft.EntityFrameworkCore;
    using RabbitMQSubscriber.Data;
    using RabbitMQSubscriber.Models;

    public class RabbitConsumerService : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private readonly IServiceProvider _provider;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly List<DataRecord> _batchBuffer = new();
        private readonly object _lock = new();
        private const int BatchSize = 30;

        public RabbitConsumerService(IOptions<RabbitMQSettings> options, IServiceProvider provider)
        {
            _settings = options.Value;
            _provider = provider;
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

            _channel.QueueDeclare(_settings.QueueName, false, false, false, null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Connect();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    lock (_batchBuffer)
                    {
                        _batchBuffer.Add(new DataRecord { Content = message });
                    }

                    if (_batchBuffer.Count >= BatchSize)
                    {
                        List<DataRecord> recordsToSave;

                        // Lock and copy buffer for thread safety
                        lock (_batchBuffer)
                        {
                            recordsToSave = _batchBuffer.ToList();
                            _batchBuffer.Clear();
                        }

                        using var scope = _provider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        db.Records.AddRange(recordsToSave);
                        await db.SaveChangesAsync();

                        Console.WriteLine($"Stored batch of {recordsToSave.Count} records.");
                    }
                };

                _channel.BasicConsume(_settings.QueueName, true, consumer);
                //Connect();

                //var consumer = new EventingBasicConsumer(_channel);
                //consumer.Received += async (_, ea) =>
                //{
                //    var body = ea.Body.ToArray();
                //    var message = Encoding.UTF8.GetString(body);

                //    using var scope = _provider.CreateScope();
                //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                //    await db.Records.AddAsync(new DataRecord { Content = message });
                //    await db.SaveChangesAsync();

                //    Console.WriteLine($"Stored: {message}");
                //};

                //_channel.BasicConsume(_settings.QueueName, true, consumer);
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
