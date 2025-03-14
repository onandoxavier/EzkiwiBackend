using VirtualQueueApi.Utils.Middlewares;

namespace VirtualQueueApi.Configuration
{
    public static class ExceptionHandlerConfig
    {
        public static IServiceCollection AddExceptionHandlerConfiguration(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }

        public static IApplicationBuilder UseExceptionHandlerConfiguration(this IApplicationBuilder app)
        {            
            app.UseExceptionHandler();
            
            return app;
        }
    }
}
