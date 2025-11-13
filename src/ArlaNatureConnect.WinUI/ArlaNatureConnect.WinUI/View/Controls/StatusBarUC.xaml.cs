using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.ViewModels.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Controls;

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
        Loaded += StatusBarUC_Loaded;
    }

    /// <summary>
    /// Handle control loaded event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StatusBarUC_Loaded(object sender, RoutedEventArgs e)
    {
        IStatusInfoServices status = App.HostInstance.Services.GetService<IStatusInfoServices>()!;

        // Indicate loading during initialization
        // On dispose of the returned IDisposable, the loading count is decremented
        using (status.BeginLoading())
        {
            if (this.DataContext is null)
            {
                // resolve viewmodel from host DI so it receives shared service
                var vm = App.HostInstance.Services.GetService<StatusBarUCViewModel>();
                if (vm is not null) this.DataContext = vm;
            }
        }
    }
}
