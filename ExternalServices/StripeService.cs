using Stripe;
using Stripe.Checkout;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Entities;
using VirtualQueueApi.Domain.Models.Queries;
using VirtualQueueApi.Domain.Models.Results.CheckoutResults;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.ExternalServices;

public class StripeService : IStripeService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ISubscriptionManagementRepository _subscriptionManagementRepository;
    private const string priceId = "";

    public StripeService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ISubscriptionManagementRepository subscriptionManagementRepository)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _subscriptionManagementRepository = subscriptionManagementRepository;
    }

    public async Task<string> CreateSession(Guid companyId, string email)
    {
        var company = await _companyRepository.Get(expression: c => c.ExternalId == companyId);
        if (company == null) throw new BusinessException("Company not found");

        var subscriptionManagementSearch = new SubscriptionManagementSearch()
        {
            CompanyId = company.Id,
            SessionStatus = ESessionStatus.Open,
            OrderBy = source => source.OrderByDescending(x => x.CreatedAt)
        };

        var subscriptionManagement = await _subscriptionManagementRepository.Get(search: subscriptionManagementSearch, track: false);
        if (subscriptionManagement != null)
        {
            var sessionOptions = new SessionGetOptions { Expand = new List<string> { "line_items" } };
            var sessionService = new SessionService();
            var openSession = sessionService.Get(subscriptionManagement.SessionId, sessionOptions);
            
            subscriptionManagement.UpdateSessionStatus(openSession.Status);

            if (subscriptionManagement.SessionStatus == ESessionStatus.Open)
            {
                subscriptionManagement.UpdateMe();
                await _subscriptionManagementRepository.SaveChanges();

                return openSession.ClientSecret;
            }            
        }

        var domain = "";

#if DEBUG
        domain = "http://localhost:5173";
#endif
        var options = new SessionCreateOptions
        {
            UiMode = "embedded",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    Price = priceId,
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            ClientReferenceId = companyId.ToString("N"),            
            CustomerEmail = email,
            ReturnUrl = domain + "/return?session_id={CHECKOUT_SESSION_ID}",
        };
        var service = new SessionService();
        Session session = service.Create(options);

        subscriptionManagement = new SubscriptionManagement(session.Id, priceId, company);
        await _subscriptionManagementRepository.Add(subscriptionManagement);

        await _subscriptionManagementRepository.SaveChanges();

        return session.ClientSecret;
    }

    public async Task<SessionStatusResult> ValidateStatus(string sessionId)
    {
        var result = new SessionStatusResult();
        
        var options = new SessionGetOptions { Expand = new List<string> { "line_items" }};
        var sessionService = new SessionService();
        Session session = sessionService.Get(sessionId, options);

        if (session == null) throw new BusinessException("Session not found");

        var subscriptionManagement = await _subscriptionManagementRepository.Get(expression: s => s.SessionId == sessionId);
        if (subscriptionManagement != null)
        {
            if (session.Status == "complete" && subscriptionManagement.SessionStatus != ESessionStatus.Complete)
            {
                subscriptionManagement.InvoiceId = session.InvoiceId;
                subscriptionManagement.SubscriptionId = session.SubscriptionId;

                await FulfillCheckoutFromSession(session, subscriptionManagement);
                result.ForceRefresh = true;                
            }

            subscriptionManagement.UpdateSessionStatus(session.Status);
            if (session.PaymentStatus != "unpaid")
                subscriptionManagement.PaymentStatus = EPaymentStatus.Paid;
        }

        result.status = session.Status;
        result.customer_email = session.CustomerDetails.Email;

        return result;
    }

    private async Task FulfillCheckoutFromSession(Session session, SubscriptionManagement subscriptionManagement)
    {
        var subscriptionService = new SubscriptionService();
        var subscription = subscriptionService.Get(subscriptionManagement.SubscriptionId);

        if (subscription == null) throw new BusinessException("Subscription not found");
        
        if (subscriptionManagement.InvoiceId != subscription.LatestInvoiceId)
        {
            subscriptionManagement.Status = ESubscriptionStatus.None;

            subscriptionManagement = new SubscriptionManagement(
                subscriptionId: subscription.Id,
                invoiceId: subscription.LatestInvoiceId,
                companyId: subscriptionManagement.CompanyId,
                priceId: subscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty);

            await _subscriptionManagementRepository.Add(subscriptionManagement);
        }

        subscriptionManagement.Start = subscription.CurrentPeriodStart;
        subscriptionManagement.End = subscription.CurrentPeriodEnd;        
        subscriptionManagement.UpdateStatus(subscription.Status ?? "");        

        await _subscriptionManagementRepository.SaveChanges();
    }

    public async Task InvoicePaid(Invoice invoice)
    {
        var subscriptionId = invoice.Subscription.Id;

        var subscriptionService = new SubscriptionService();
        var subscription = subscriptionService.Get(subscriptionId);

        await UpdateSubscription(subscription);
    }

    public async Task UpdateSubscription(Subscription subscription)
    {
        var subscriptionItem = subscription.Items.Data.FirstOrDefault();
        var priceId = subscriptionItem?.Price?.Id ?? "";

        var subscriptionManagementSearch = new SubscriptionManagementSearch
        {
            SubscriptionId = subscription.Id,
            InvoiceId = subscription.LatestInvoiceId
        };
        var subscriptionManagement = await _subscriptionManagementRepository.Get(search: subscriptionManagementSearch);
        if (subscriptionManagement == null)
        {
            int companyId = 0;

            var lastSubscriptionSearch = new SubscriptionManagementSearch
            {
                SubscriptionId = subscription.Id,
                OrderBy = source => source.OrderByDescending(x => x.CreatedAt)
            };

            var lastSubscription = await _subscriptionManagementRepository.Get(search: lastSubscriptionSearch);
            if (lastSubscription == null) 
            {
                var customerService = new CustomerService();
                var customer = customerService.Get(subscription.CustomerId);
                if (customer == null) throw new BusinessException("Customer not found");

                var user = await _userRepository.Get(expression: u => u.Email == customer.Email);
                if (user == null) throw new BusinessException("User not found");

                companyId = user.CompanyId;
            }
            else
                companyId = lastSubscription.CompanyId;

            subscriptionManagement = new SubscriptionManagement(
                subscriptionId: subscription.Id,
                invoiceId: subscription.LatestInvoiceId,
                companyId: companyId,
                priceId: priceId
            );

            await _subscriptionManagementRepository.Add(subscriptionManagement);
        }

        subscriptionManagement.UpdateStatus(subscription.Status);
        subscriptionManagement.Start = subscription.CurrentPeriodStart;
        subscriptionManagement.End = subscription.CurrentPeriodEnd;

        await _subscriptionManagementRepository.SaveChanges();
    }
}
