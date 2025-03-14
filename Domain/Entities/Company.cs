using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Models.Entities;

public class Company : EntityBase<int>
{
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public Company(string name) { Name = name; }
}
