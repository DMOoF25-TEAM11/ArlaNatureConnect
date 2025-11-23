using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.Extensions.DependencyInjection;
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

        // Try to resolve the ViewModel from the app's DI container at runtime.
        // If DI isn't available (design-time or tests) fall back to parameterless VM for designer support.
        try
        {
            StatusBarUCViewModel? vm = App.HostInstance?.Services.GetService<StatusBarUCViewModel>();
            if (vm != null)
            {
                DataContext = vm;
            }
            else
            {
                // Keep parameterless VM for design-time and testability
                DataContext = new StatusBarUCViewModel();
            }
        }
        catch
        {
            // Swallow resolution errors and fallback to design-time VM instance
            DataContext = new StatusBarUCViewModel();
        }
    }
}
