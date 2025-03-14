using Stripe;
using VirtualQueueApi.Domain.Models.Results.CheckoutResults;

namespace VirtualQueueApi.Domain.Contracts.Services;

public interface IStripeService
{
    Task<string >CreateSession(Guid companyId, string email);
    Task<SessionStatusResult> ValidateStatus(string sessionId);
    Task UpdateSubscription(Subscription subscription);
    Task InvoicePaid(Invoice invoice);
}
