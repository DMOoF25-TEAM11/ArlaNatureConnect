using ArlaNatureConnect.WinUI.Dialogs;
using ArlaNatureConnect.WinUI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System.Diagnostics;

namespace ArlaNatureConnect.WinUI;

public sealed partial class StartWindow : Window
{
    private readonly StartWindowViewModel _viewModel;
    private readonly TaskCompletionSource<bool> _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Danish translations as private consts
    private const string _connectionFailedTitle = "Databaseforbindelse mislykkedes";
    private const string _connectionFailedContentSuffix = "\n\nKontroller venligst servernavn, netværksforbindelse og legitimationsoplysninger.";
    private const string _retryButtonText = "Prøv igen";
    private const string _cancelButtonText = "Annuller";

    // Public task the host can await to know when startup (including connection dialog) is done
    public Task Initialization => _initializationTcs.Task;

    public StartWindow()
    {
        _viewModel = App.HostInstance.Services.GetRequiredService<StartWindowViewModel>();

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
            await _viewModel.InitializeAsync(ShowConnectionDialogAndSaveAsync, ShowConnectionErrorAsync);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during initialization: {ex}");
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
            Title = _connectionFailedTitle,
            Content = message + _connectionFailedContentSuffix,
            PrimaryButtonText = _retryButtonText,
            CloseButtonText = _cancelButtonText,
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
                await _viewModel.SaveConnectionStringAsync(cs);
            }
        }
    }
}
