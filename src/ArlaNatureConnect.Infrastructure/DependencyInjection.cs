using ArlaNatureConnect.Core;
using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Abstract.Services;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;
using ArlaNatureConnect.Infrastructure.Services;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddCoreServices();

        string connectionString = services.BuildServiceProvider().GetRequiredService<IConnectionStringService>().ReadAsync().Result ?? string.Empty;

        // Register the EF Core DbContext factory after validation
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));

        //services.AddSingleton<IConnectionStringService, ConnectionStringService>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();

        // Query services
        services.AddScoped<IPersonQueryService, PersonQueryService>();

        return services;
    }
}
