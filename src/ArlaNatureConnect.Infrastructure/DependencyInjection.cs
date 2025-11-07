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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        // Retrieve connection string from the ConnectionStringService
        var connectionStringService = new ConnectionStringService();
        string? connectionString = connectionStringService.ReadAsync().GetAwaiter().GetResult();

        // Force-enable MARS to allow multiple active readers on the same connection
        var csb = new SqlConnectionStringBuilder(connectionString)
        {
            MultipleActiveResultSets = true
        };

        // register infrastructure services here
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(csb.ConnectionString));
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();
        return services;
    }
}
