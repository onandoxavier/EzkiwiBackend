namespace VirtualQueueApi.Domain.Models.Results.QueueResults
{
    public class QueueListResult
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;

        public QueueListResult(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
