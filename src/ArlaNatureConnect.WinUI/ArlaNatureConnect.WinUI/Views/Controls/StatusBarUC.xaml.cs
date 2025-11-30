using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

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

        // Resolve the view-model from the application's DI container now.
        StatusBarUCViewModel vm = App.HostInstance.Services.GetRequiredService<StatusBarUCViewModel>();

        // Initialize the viewmodel for UI use from this UI thread
        vm.InitializeForUi(DispatcherQueue.GetForCurrentThread());

        DataContext = vm;
    }
}
