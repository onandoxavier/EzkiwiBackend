using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Models.Entities;

public class PasswordHistory : EntityBase<int>
{
    public string Value { get; set; }
    public int QueueId { get; set; }
    public Queue Queue { get; set; }
}
