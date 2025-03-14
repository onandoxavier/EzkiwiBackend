using System.Net;
using System.Threading.RateLimiting;
using VirtualQueueApi.ExternalServices;

namespace VirtualQueueApi.Configuration
{
    public static class RateLimitingConfig
    {
        public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
                {
                    IPAddress remoteIpAddress = context.Connection.RemoteIpAddress;

                    if (!IPAddress.IsLoopback(remoteIpAddress!))
                    {
                        return RateLimitPartition.GetTokenBucketLimiter(remoteIpAddress!, _ =>
                            new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 20, // Permite até 20 requisições
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5, // Permite até 5 requisições na fila
                            ReplenishmentPeriod = TimeSpan.FromSeconds(30), // 30 segundos de recarga
                            TokensPerPeriod = 20, // 20 tokens recarregados a cada 30 segundos
                        });
                    }

                    return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var httpContext = context.HttpContext;
                    var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("RateLimiter");


                    var blockedIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    logger.LogWarning("IP {BlockedIp} bloqueado pelo RateLimiter", blockedIp);

                    httpContext.Response.ContentType = "text/plain";
                    await httpContext.Response.WriteAsync("Too many requests, você foi rate-limited!", cancellationToken);

                    EmailService.SendEmail(
                        receiverEmail: "", receiverName: "", 
                        subject: "limite de reqs atingido", message: $"IP {blockedIp} bloqueado pelo RateLimiter");
                };
            });

            return services;
        }

        public static IApplicationBuilder UseRateLimitingConfiguration(this IApplicationBuilder app)
        {
            app.UseRateLimiter();
            return app;
        }
    }
}
