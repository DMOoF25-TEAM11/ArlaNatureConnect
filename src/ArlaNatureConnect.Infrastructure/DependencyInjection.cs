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
        ConnectionStringService connectionStringService = new();

        // Avoid deadlock when calling async IO from a context-bound thread (UI thread).
        // Run the async read on the thread-pool so its awaits won't attempt to resume on the UI sync context.
        string? connectionString = Task.Run(() => connectionStringService.ReadAsync()).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No connection string found. Ensure the StartWindow connection dialog was completed before building the host.");
        }

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
        catch (Exception ex)
        {
            throw new InvalidOperationException("The connection string is invalid or not found.", ex);
        }

        // Register infrastructure services here
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();

        return services;
    }
}
