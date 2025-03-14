using VirtualQueueApi.Domain.Entities;

namespace VirtualQueueApi.Models.Entities;

public class Queue : EntityBase<int>
{
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public Company Company { get; set; }

    public ICollection<PasswordHistory> PasswordHistories { get; set; } = [];

    public Queue() { }
    public Queue(string name, Company company)
    {
        Name = name;
        Company = company;
    }
}

