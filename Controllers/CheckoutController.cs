using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Utils.Extensions;
// Não tirar a referência abaixo
// trecho só é chamado em produção
using VirtualQueueApi.ExternalServices;

namespace VirtualQueueApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{

    //Webhook
    private readonly string secret;
    private readonly IStripeService _stripeService;

    public CheckoutController(IStripeService stripeService, IConfiguration configuration)
    {
        secret = configuration["StrikeService:webHookSecret"] ?? "";
        _stripeService = stripeService;
    }

    [HttpPost("create-session", Name = "CreateSession")]
    public async Task<IActionResult> Create()
    {
        var companyId = User.GetCompanyId();
        var email = User.GetEmail();

        var clientSecret = await _stripeService.CreateSession(companyId, email);

        return Ok(new { clientSecret = clientSecret });
    }

    [HttpGet("session-status", Name = "SessionStatus")]
    public async Task<IActionResult> SessionStatus([FromQuery] string session_id)
    {
        var result = await _stripeService.ValidateStatus(session_id);

        return Ok(result);
    }    
        
    [AllowAnonymous]
    [HttpPost("Stripe", Name = "StripeWebhookCheckout")]
    public async Task<IActionResult> StripeWebhookCheckout()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();        

        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            secret
        );

#if !DEBUG        
        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted
            || stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded
            || stripeEvent.Type == EventTypes.CustomerSubscriptionUpdated
            || stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted
            || stripeEvent.Type == EventTypes.InvoicePaid)
            EmailService.SendStripeWebHook(json, stripeEvent.Type);
#endif

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted
            || stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentSucceeded
            || stripeEvent.Type == EventTypes.CheckoutSessionAsyncPaymentFailed
            || stripeEvent.Type == EventTypes.CheckoutSessionExpired)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return BadRequest();

            await _stripeService.ValidateStatus(session.Id);
        }
        else if (stripeEvent.Type == EventTypes.CustomerSubscriptionUpdated
            || stripeEvent.Type == EventTypes.CustomerSubscriptionDeleted)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return BadRequest();
            
            await _stripeService.UpdateSubscription(subscription);
        }
        else if (stripeEvent.Type == EventTypes.InvoicePaid)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return BadRequest();

            await _stripeService.InvoicePaid(invoice);
        }

        return Ok();      
    }
}
