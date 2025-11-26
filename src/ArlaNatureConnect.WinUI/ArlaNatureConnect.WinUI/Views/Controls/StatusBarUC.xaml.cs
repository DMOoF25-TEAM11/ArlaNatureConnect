using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls;

/// <summary>
/// A status bar user control.
/// Provides a central UI element for reporting app-wide status (loading and database connection)
/// so different components can signal state consistently and the user receives reliable feedback.
/// </summary>
public sealed partial class StatusBarUC : UserControl
{
    public StatusBarUC()
    {
        InitializeComponent();

        // Try to resolve the view-model from the application's DI container now. If the host isn't built yet
        // (for example during design-time or early startup), defer resolution until the control is loaded.
        if (App.HostInstance?.Services != null)
        {
            StatusBarUCViewModel vm = App.HostInstance.Services.GetRequiredService<StatusBarUCViewModel>();
            DataContext = vm;
        }
        else
        {
            Loaded += StatusBarUC_Loaded;
        }
    }

    private void StatusBarUC_Loaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= StatusBarUC_Loaded;
        try
        {
            if (App.HostInstance?.Services != null)
            {
                StatusBarUCViewModel vm = App.HostInstance.Services.GetRequiredService<StatusBarUCViewModel>();
                DataContext = vm;
            }
        }
        catch { /* swallow to avoid throwing in UI */ }
    }
}
