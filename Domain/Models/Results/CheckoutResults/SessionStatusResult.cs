namespace VirtualQueueApi.Domain.Models.Results.CheckoutResults;

public class SessionStatusResult
{
    public string status { get; set; } = string.Empty;
    public string customer_email { get; set; } = string.Empty;
    public bool ForceRefresh { get; set; }
}
