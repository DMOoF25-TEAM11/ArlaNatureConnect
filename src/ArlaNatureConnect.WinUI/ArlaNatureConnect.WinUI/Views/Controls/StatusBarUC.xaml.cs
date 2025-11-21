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
    public StatusBarUC()
    {
        InitializeComponent();
    }
}
