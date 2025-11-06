using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        
        return services;
    }
}
