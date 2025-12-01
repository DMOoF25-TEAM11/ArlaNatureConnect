using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;

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
        : this(App.HostInstance.Services.GetRequiredService<StatusBarUCViewModel>())
    {
    }

    /// <summary>
    /// Constructor-injecting ViewModel. Use this in tests or when the VM is provided externally.
    /// </summary>
    /// <param name="viewModel">The view-model to use as DataContext.</param>
    public StatusBarUC(StatusBarUCViewModel viewModel)
    {
        // InitializeComponent requires WinUI/WinRT activation to be available. In unit test
        // environments that don't have the Windows App SDK registered this will throw a
        // COMException. Catch that so tests can construct the control and verify non-UI
        // behavior (like DataContext wiring) without requiring the full runtime.
        try
        {
            InitializeComponent();
        }
        catch (COMException)
        {
            // Running in a non-WinUI-capable test environment; skip XAML initialization.
        }

        // Initialize the viewmodel for UI use from this UI thread if needed
        // viewModel.InitializeForUi(DispatcherQueue.GetForCurrentThread());

        DataContext = viewModel;
    }
}
