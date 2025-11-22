using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Dialogs;
using ArlaNatureConnect.WinUI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.Diagnostics;

using Windows.Storage;
using Microsoft.Data.SqlClient;

namespace ArlaNatureConnect.WinUI;

public sealed partial class StartWindow : Window
{
    private readonly StartWindowViewModel _viewModel;
    private readonly TaskCompletionSource<bool> _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Public task the host can await to know when startup (including connection dialog) is done
    public Task Initialization => _initializationTcs.Task;

    // Accept an optional IConnectionStringService so the caller (App) can pass the same instance
    public StartWindow(IConnectionStringService? connService = null)
    {
        var cs = connService ?? new ConnectionStringService();
        _viewModel = new StartWindowViewModel(cs);

        InitializeComponent();

        // Attach to the root content Loaded event if available
        if (Content is FrameworkElement fe)
        {
            fe.Loaded += StartWindow_Loaded;
        }
        else
        {
            // Fallback: call the async loader directly
            StartWindow_Loaded(null, null);
        }
    }

    private async void StartWindow_Loaded(object? sender, RoutedEventArgs? e)
    {
        try
        {
            // Try packaged-app URI first (works only if app is packaged)
            Uri uri = new Uri("ms-appx:///Assets/startUpImage.jpg");
            try
            {
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
                //System.Diagnostics.Debug.WriteLine($"Found (packaged): {f.Path}");
            }
            catch
            {
                // ignore and try file system fallback
            }

            // Fallback for unpackaged / desktop scenarios: check output folder
            string filePath = System.IO.Path.Combine(AppContext.BaseDirectory ?? string.Empty, "Assets", "startUpImage.jpg");
            if (System.IO.File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Found (fs): {filePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Image not found at either ms-appx or output folder: {filePath}");
                throw new FileNotFoundException("Startup image not found");
            }

            // Check for existing connection string
            bool exists = await _viewModel.ConnectionExistsAsync();
            string? conn = null;
            if (!exists)
            {
                await ShowConnectionDialogAndSaveAsync();
            }
            else
            {
                conn = await _viewModel.ReadConnectionStringAsync();
                if (string.IsNullOrEmpty(conn))
                {
                    await ShowConnectionDialogAndSaveAsync();
                }
            }



            // If there is a connection string, validate it asynchronously and let user retry on failure
            if (!string.IsNullOrWhiteSpace(conn))
            {
                var (isValid, _) = await _viewModel.ValidateConnectionStringWithRetryAsync(conn);
                if (!isValid)
                {
                    // Let user re-enter connection string
                    await ShowConnectionDialogAndSaveAsync();
                    conn = await _viewModel.ReadConnectionStringAsync();
                    if (!string.IsNullOrWhiteSpace(conn))
                    {
                        await _viewModel.ValidateConnectionStringWithRetryAsync(conn);
                    }
                }
            }

            // Login dialog should be done here


        }

        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading startup image: {ex}");
        }
        finally
        {
            // Mark initialization complete regardless of result so host won't wait forever
            _initializationTcs.TrySetResult(true);
        }
    }

    private async Task ShowConnectionErrorAsync(string message)
    {
        // Show a simple content dialog to inform the user and allow retry
        ContentDialog dialog = new ContentDialog()
        {
            Title = "Database connection failed",
            Content = message + "\n\nPlease check the server name, network connectivity, and credentials.",
            PrimaryButtonText = "Retry",
            CloseButtonText = "Cancel",
            XamlRoot = (Content as FrameworkElement)?.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            // No-op here; caller will re-open the connection dialog if needed
        }
    }


    private async Task ShowConnectionDialogAndSaveAsync()
    {
        ConnectionDialog dialog = new ConnectionDialog();
        // ensure the dialog is hosted correctly
        dialog.XamlRoot = (Content as FrameworkElement)?.XamlRoot;

        ConnectionDialogViewModel vm = dialog.DataContext as ConnectionDialogViewModel ?? new ConnectionDialogViewModel();

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            string? cs = vm.ConnectionString?.Trim();
            if (!string.IsNullOrEmpty(cs))
            {
                Debug.WriteLine($"Saving connection string: {cs}");
                Debug.WriteLine($"Connection string length: {cs.Length}");
                await _viewModel.SaveConnectionStringAsync(cs);
            }
        }
    }
}
