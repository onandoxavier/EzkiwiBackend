namespace VirtualQueueApi.Domain.Models.Results.AuthResults
{
    public class NewPasswordResult
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Value { get; set; } = string.Empty;
        public Guid QueueId { get; set; } = Guid.Empty;

        public NewPasswordResult(int id, DateTimeOffset createdAt, string value, Guid queueId)
        {
            Id = id;
            CreatedAt = createdAt;
            Value = value;
            QueueId = queueId;
        }

        public override string ToString()
        {
            return QueueId + " - " + Value;
        }
    }
}
