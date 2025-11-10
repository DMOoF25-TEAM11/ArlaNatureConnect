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

        // Check if connection string is valid, otherwise use in-memory database as fallback
        bool useInMemory = string.IsNullOrWhiteSpace(connectionString);
        
        if (!useInMemory)
        {
            try
            {
                // Try to validate the connection string by creating a SqlConnectionStringBuilder
                var csb = new SqlConnectionStringBuilder(connectionString)
                {
                    MultipleActiveResultSets = true
                };
                
                // Register SQL Server database
                services.AddDbContext<AppDbContext>(options => options.UseSqlServer(csb.ConnectionString));
            }
            catch
            {
                // If connection string is invalid, fall back to in-memory database
                useInMemory = true;
            }
        }

        // Use in-memory database as fallback if connection string is missing or invalid
        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ArlaNatureConnectDb"));
        }

        // Register infrastructure services here
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();
        
        return services;
    }
}
