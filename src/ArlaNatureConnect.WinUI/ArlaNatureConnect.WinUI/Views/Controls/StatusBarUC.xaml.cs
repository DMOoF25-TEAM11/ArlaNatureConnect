using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls;

/// <summary>
/// A status bar user control.
/// Provides a central UI element for reporting app-wide status (loading and database connection)
/// so different components can signal state consistently and the user receives reliable feedback.
/// </summary>
public sealed partial class StatusBarUC : UserControl
{
    /// <summary>
    /// Production/default constructor: resolve VM from the application's DI container.
    /// Chains to the VM-injecting constructor so all initialization is centralized.
    /// </summary>
    public StatusBarUC()
    {
        InitializeComponent();

        // Initialize the viewmodel for UI use from this UI thread if needed
        DataContext = App.HostInstance?.Services.GetService(typeof(StatusBarUCViewModel)) as StatusBarUCViewModel
            ?? throw new InvalidOperationException("Application host not initialized or StatusBarUCViewModel not registered in DI.");

        // Ensure the view-model hooks into the status service and performs initial update
        (DataContext as StatusBarUCViewModel)?.InitializeForUi();
    }
}
