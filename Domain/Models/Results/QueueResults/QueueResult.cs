namespace VirtualQueueApi.Domain.Models.Results.QueueResults;

public class QueueResult
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;

    public QueueResult(Guid id, string name, string company)
    {
        Id = id;
        Name = name;
        Company = company;
    }
}
