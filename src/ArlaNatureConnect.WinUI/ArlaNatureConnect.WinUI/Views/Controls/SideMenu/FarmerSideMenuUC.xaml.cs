using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Farmer sidebar navigation menu.
/// View-specific logic is minimal; common behavior is provided by UCSideMenu base defined in XAML.
/// </summary>
public sealed partial class FarmerSideMenuUC
{
    public FarmerSideMenuUC()
    {
        InitializeComponent();
        Loaded += FarmerSideMenuUC_Loaded;
    }

    private void FarmerSideMenuUC_Loaded(object? sender, RoutedEventArgs e)
    {
        // Use delegates as CommandParameter so the NavigationCommand can execute page-level or computed logic.
        // These will override the static CommandParameter values declared in XAML.
        DashboardsButton.CommandParameter = new System.Func<string>(() => "Dashboards");
        FarmsButton.CommandParameter = new System.Func<string>(() => "Farms");
        TasksButton.CommandParameter = new System.Func<string>(() => "Tasks");
    }
}

