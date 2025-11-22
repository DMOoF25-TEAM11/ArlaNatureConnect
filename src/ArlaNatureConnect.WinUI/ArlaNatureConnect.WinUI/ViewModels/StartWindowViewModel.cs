using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ArlaNatureConnect.Core.Services;
using System.IO;
using System.Diagnostics;
using Windows.Storage;

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

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
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
                using SqlConnection conn = new SqlConnection(builder.ConnectionString);
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

    /// <summary>
    /// Perform startup checks previously performed in StartWindow.Loaded.
    /// UI interactions (showing dialogs) are delegated to the caller via provided callbacks.
    /// </summary>
    /// <param name="showConnectionDialogAndSaveAsync">Delegate used to show the connection dialog and save the connection string when needed.</param>
    /// <param name="showConnectionErrorAsync">Delegate used to show connection error messages to the user.</param>
    public async Task InitializeAsync(Func<Task> showConnectionDialogAndSaveAsync, Func<string, Task> showConnectionErrorAsync)
    {
        try
        {
            // Try packaged-app URI first (works only if app is packaged)
            Uri uri = new Uri("ms-appx:///Assets/startUpImage.jpg");
            try
            {
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
                // Found in package
            }
            catch
            {
                // ignore and try file system fallback
            }

            // Fallback for unpackaged / desktop scenarios: check output folder
            string filePath = Path.Combine(AppContext.BaseDirectory ?? string.Empty, "Assets", "startUpImage.jpg");
            if (File.Exists(filePath))
            {
                Debug.WriteLine($"Found (fs): {filePath}");
            }
            else
            {
                Debug.WriteLine($"Image not found at either ms-appx or output folder: {filePath}");
                throw new FileNotFoundException("Startup image not found");
            }

            // Check for existing connection string
            bool exists = await ConnectionExistsAsync();
            string? conn = null;
            if (!exists)
            {
                if (showConnectionDialogAndSaveAsync != null) await showConnectionDialogAndSaveAsync();
            }
            else
            {
                conn = await ReadConnectionStringAsync();
                if (string.IsNullOrEmpty(conn))
                {
                    if (showConnectionDialogAndSaveAsync != null) await showConnectionDialogAndSaveAsync();
                }
            }

            // If there is a connection string, validate it asynchronously and let user retry on failure
            if (!string.IsNullOrWhiteSpace(conn))
            {
                var (isValid, _) = await ValidateConnectionStringWithRetryAsync(conn);
                if (!isValid)
                {
                    // Let user re-enter connection string
                    if (showConnectionDialogAndSaveAsync != null) await showConnectionDialogAndSaveAsync();
                    conn = await ReadConnectionStringAsync();
                    if (!string.IsNullOrWhiteSpace(conn))
                    {
                        await ValidateConnectionStringWithRetryAsync(conn);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading startup image: {ex}");
            if (showConnectionErrorAsync != null)
            {
                await showConnectionErrorAsync(ex.Message);
            }
        }
    }
}
