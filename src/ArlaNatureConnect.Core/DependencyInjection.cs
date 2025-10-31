using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // register core application services here

        return services;
    }
}
