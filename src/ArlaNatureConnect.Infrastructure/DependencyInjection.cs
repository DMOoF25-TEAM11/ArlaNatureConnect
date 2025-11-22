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

        // resolve the registered IConnectionStringService from the service collection only if provided
        if (cs is not null)
        {
            // call the async method synchronously without an extra Task.Run wrapper
            // Use ConfigureAwait(false) to avoid capturing the synchronization context and potential deadlocks
            string? connectionString = cs.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No connection string found. Ensure the StartWindow connection dialog was completed before building the host.");

            // Basic validation: ensure Data Source / Server is present — prevent saving an incomplete connection string
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                if (string.IsNullOrWhiteSpace(builder.DataSource))
                {
                    throw new InvalidOperationException("Connection string is missing a server/data source. Please configure a valid SQL Server instance in the connection dialog.");
                }

                // Do not attempt to open a test connection here during DI/host build. Connection validation is performed asynchronously in StartWindow
                // to allow user interaction and retries without failing host construction.
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("Provided connection string is invalid. Please check and enter a valid SQL Server connection string.");
            }

            // Register the EF Core DbContext factory after validation
            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
        }

        //services.AddSingleton<IConnectionStringService, ConnectionStringService>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();

        return services;
    }
}
