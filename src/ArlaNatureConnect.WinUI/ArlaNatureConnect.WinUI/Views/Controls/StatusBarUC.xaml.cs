using ArlaNatureConnect.Core.Services;
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
    private readonly IStatusInfoServices _statusInfoServices;

    public StatusBarUC()
    {
        this.InitializeComponent();
        if (App.HostInstance != null)
        {
            this.DataContext = App.HostInstance.Services.GetService<StatusBarUCViewModel>();
            _statusInfoServices = App.HostInstance.Services.GetRequiredService<IStatusInfoServices>();
        }
        else
        {
            // Handle the case where HostInstance is null, e.g., throw or assign a default/mock
            throw new InvalidOperationException("App.HostInstance is not initialized.");
        }
    }
}
