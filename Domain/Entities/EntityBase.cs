namespace VirtualQueueApi.Domain.Entities;

public class EntityBase<T>
{
    public T Id { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public void UpdateMe() => UpdatedAt = DateTimeOffset.UtcNow;
}