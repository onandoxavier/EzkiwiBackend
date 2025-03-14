using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Entities;

public class SubscriptionManagement :  EntityBase<Guid>
{
    public string SessionId { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public string? SubscriptionId { get; set; }
    public string? InvoiceId { get; set; }    
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? End { get; set; }
    public ESessionStatus SessionStatus{ get; set; } = ESessionStatus.None;
    public EPaymentStatus PaymentStatus { get; set; } = EPaymentStatus.None;
    public ESubscriptionStatus Status { get; set; } = ESubscriptionStatus.None;

    public int CompanyId { get; set; }
    public virtual Company Company { get; set; }

    public SubscriptionManagement() { }

    public SubscriptionManagement(string subscriptionId, string invoiceId, int companyId, string priceId)
    {
        Id = Guid.NewGuid();

        InvoiceId = invoiceId;
        SubscriptionId = subscriptionId;
        PriceId = priceId;

        CompanyId = companyId;
    }

    public SubscriptionManagement(string sessionId, string priceId, Company company) 
    {
        Id = Guid.NewGuid();

        Company = company;
        PriceId = priceId;
        SessionId = sessionId;

        SessionStatus = ESessionStatus.Open;
    }

    public void UpdateSessionStatus(string sessionStatus)
    {
        SessionStatus = sessionStatus switch
        {
            "complete" => ESessionStatus.Complete,
            "open" => ESessionStatus.Open,
            "expired" => ESessionStatus.Expired,
            _ => ESessionStatus.None
        };

        UpdateMe();
    }

    public void UpdateStatus(string status)
    {
        Status = status switch
        {
            "incomplete" => ESubscriptionStatus.Incomplete,
            "incomplete_expired" => ESubscriptionStatus.IncompleteExpired,
            "trialing" => ESubscriptionStatus.Trialing,
            "active" => ESubscriptionStatus.Active,
            "past_due" => ESubscriptionStatus.PastDue,
            "canceled" => ESubscriptionStatus.Canceled,
            "unpaid" => ESubscriptionStatus.Unpaid,
            "paused" => ESubscriptionStatus.Paused,
            _ => ESubscriptionStatus.None
        };

        UpdateMe();
    }
}

public enum ESubscriptionStatus
{
    None = 0,
    Incomplete = 1,
    IncompleteExpired = 2,
    Trialing = 3,
    Active = 4,
    PastDue = 5,
    Canceled = 6,
    Unpaid = 7,
    Paused = 8,
}

public enum ESessionStatus
{
    None = 0,
    Open = 1,
    Complete = 2,
    Expired = 3,
}

public enum EPaymentStatus 
{
    None = 0,
    Paid = 1,
    Unpaid = 2
}