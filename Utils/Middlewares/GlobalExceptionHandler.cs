using Stripe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using VirtualQueueApi.Utils.Exceptions;

namespace VirtualQueueApi.Utils.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "{Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error"
        };

        if (exception is BusinessException businessException)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Business Error";
            problemDetails.Extensions = new Dictionary<string, object?>
            {
                { "errors", new[] { businessException.Message } }
            };
        }
        else if (exception is UnauthorizedException unauthorizedException)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Authorization Error";
            problemDetails.Extensions = new Dictionary<string, object?>
            {
                { "errors", new[] { unauthorizedException.Message } }
            };
        }
        else if (exception is EntityNotFoundException entityNotFoundException)
        {
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Title = "Entity Not Found";
            problemDetails.Extensions = new Dictionary<string, object?>
            {
                { "errors", new[] { entityNotFoundException.Message } }
            };
        }
        else if (exception is StripeException stripeException)
        {
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Stripe Error";
            problemDetails.Extensions = new Dictionary<string, object?>
            {
                { "errors", new[] { stripeException.Message } }
            };
        }
        //else if (exception is EntityValidationException entityValidationException)
        //{
        //    problemDetails.Status = StatusCodes.Status400BadRequest;
        //    problemDetails.Title = "Validation Error";
        //    problemDetails.Extensions = new Dictionary<string, object?>
        //    {
        //        { "errors", new[] { entityValidationException.Message.Split(";") } }
        //    };
        //}
        else
        {
            problemDetails.Detail = exception.Message;
#if !DEBUG
            problemDetails.Detail = "An unexpected error occurred.";
#endif
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
