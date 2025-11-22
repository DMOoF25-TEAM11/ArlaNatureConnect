using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ArlaNatureConnect.Core.Services;

namespace ArlaNatureConnect.WinUI.ViewModels;

public class StartWindowViewModel
{
    private readonly IConnectionStringService _connService;

    public StartWindowViewModel(IConnectionStringService connService)
    {
        _connService = connService ?? throw new ArgumentNullException(nameof(connService));
    }

    public Task<bool> ConnectionExistsAsync() => _connService.ExistsAsync();

    public Task<string?> ReadConnectionStringAsync() => _connService.ReadAsync();

    public Task SaveConnectionStringAsync(string connectionString) => _connService.SaveAsync(connectionString);

    /// <summary>
    /// Validates the provided connection string by checking the DataSource and attempting to open a connection.
    /// Returns a tuple indicating success and an optional error message suitable for display.
    /// This method is UI-agnostic to allow unit testing.
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateConnectionStringWithRetryAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, "Connection string is empty.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
            return (false, "Connection string is missing a server/data source. Please enter a valid SQL Server instance.");

        // Ensure a short timeout for the UI test
        int originalTimeout = builder.ConnectTimeout;
        if (originalTimeout <= 0 || originalTimeout > 10)
        {
            builder.ConnectTimeout = 5;
        }

        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var conn = new SqlConnection(builder.ConnectionString);
                await conn.OpenAsync();
                await conn.CloseAsync();
                return (true, null);
            }
            catch (SqlException ex)
            {
                if (attempt == maxAttempts)
                {
                    return (false, $"Unable to connect to SQL Server '{builder.DataSource}': {ex.Message}");
                }
                await Task.Delay(500);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (attempt == maxAttempts)
                {
                    return (false, $"Network error while attempting to connect to SQL Server '{builder.DataSource}': {ex.Message}");
                }
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error while validating connection: {ex.Message}");
            }
        }

        return (false, "Unknown error validating connection string.");
    }
}
