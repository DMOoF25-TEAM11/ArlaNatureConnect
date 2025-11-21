using ArlaNatureConnect.Core;
using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Infrastructure.Repositories;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConnectionStringService? cs = null)
    {
        services.AddCoreServices();

        // resolve the registered IConnectionStringService from the service collection only if not provided
        if (cs is null)
        {
            using ServiceProvider tempProvider = services.BuildServiceProvider();
            cs = tempProvider.GetService<IConnectionStringService>() ?? throw new InvalidOperationException("IConnectionStringService is not registered in the service collection.");
        }

        // call the async method synchronously without an extra Task.Run wrapper
        string? connectionString = cs.ReadAsync().GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("No connection string found. Ensure the StartWindow connection dialog was completed before building the host.");

        SqlConnectionStringBuilder csb = new(connectionString) { MultipleActiveResultSets = true };
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(csb.ConnectionString));

        //services.AddSingleton<IConnectionStringService, ConnectionStringService>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();

        return services;
    }
}
