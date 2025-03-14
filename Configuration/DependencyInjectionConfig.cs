using VirtualQueueApi.Data;
using VirtualQueueApi.Data.Repositories;
using VirtualQueueApi.Domain.Contracts.Repositories;
using VirtualQueueApi.Domain.Contracts.Services;
using VirtualQueueApi.Domain.Contracts.Transactions;
using VirtualQueueApi.ExternalServices;
using VirtualQueueApi.Services;

namespace VirtualQueueApi.Configuration;

public static class DependencyInjectionConfig
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQueueRepository, QueueRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetFlowRepository, PasswordResetFlowRepository>();
        services.AddScoped<ISubscriptionManagementRepository, SubscriptionManagementRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IStripeService, StripeService>();
    }
}
