namespace RabbitMQSubscriber.Services
{
    public class DataRecord
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
