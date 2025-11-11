using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Dialogs;
using ArlaNatureConnect.WinUI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Storage;

namespace ArlaNatureConnect.WinUI;

public sealed partial class StartWindow : Window
{
    private readonly IConnectionStringService _connService = new ConnectionStringService();
    private readonly TaskCompletionSource<bool> _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Public task the host can await to know when startup (including connection dialog) is done
    public Task Initialization => _initializationTcs.Task;

    public StartWindow()
    {
        InitializeComponent();

        // Attach to the root content Loaded event if available
        if (this.Content is FrameworkElement fe)
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
            var uri = new Uri("ms-appx:///Assets/startUpImage.jpg");
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
            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory ?? string.Empty, "Assets", "startUpImage.jpg");
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
            bool exists = await _connService.ExistsAsync();
            string? conn = null;
            if (!exists)
            {
                await ShowConnectionDialogAndSaveAsync();
            }
            else
            {
                conn = await _connService.ReadAsync();
                if (string.IsNullOrEmpty(conn))
                {
                    await ShowConnectionDialogAndSaveAsync();
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


    private async Task ShowConnectionDialogAndSaveAsync()
    {
        var dialog = new ConnectionDialog();
        // ensure the dialog is hosted correctly
        dialog.XamlRoot = (this.Content as FrameworkElement)?.XamlRoot;

        ConnectionDialogViewModel vm = dialog.DataContext as ConnectionDialogViewModel ?? new ConnectionDialogViewModel();

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var cs = vm.ConnectionString?.Trim();
            if (!string.IsNullOrEmpty(cs))
            {
                await _connService.SaveAsync(cs);
            }
        }
    }
}
