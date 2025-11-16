using ArlaNatureConnect.Core.Services;

using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // register core application services here
        // Status info is app-wide state; register as singleton so views and viewmodels share the same instance
        services.AddSingleton<IStatusInfoServices, StatusInfoService>();
        services.AddSingleton<IAppMessageService, AppMessageService>();
        services.AddSingleton<IConnectionStringService, ConnectionStringService>();

        return services;
    }
}
