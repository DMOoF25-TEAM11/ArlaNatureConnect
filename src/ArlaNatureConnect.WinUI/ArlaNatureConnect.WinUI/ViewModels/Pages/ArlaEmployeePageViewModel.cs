using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Farmer;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ArlaEmployeePage - handles dashboard display for Arla employees.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via Initialize()
/// - Stores the selected role for future use
/// - Handles navigation between different sections (Dashboards, Farms, Users)
/// - Prepares dashboard display for Arla employees
/// 
/// Usage:
/// This ViewModel is used by ArlaEmployeePage.xaml to display the dashboard for Arla employees.
/// Unlike FarmerPage and ConsultantPage, Arla employees do not need to select a specific
/// user, as they have direct access to the dashboard. The ViewModel handles navigation between
/// different content views and initialization with the role.
/// </summary>
public class ArlaEmployeePageViewModel : NavigationViewModelBase
{

    public ArlaEmployeePageViewModel(NavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        RegisterContent("Dashboards", () => new FarmerDashboards());
        RegisterContent("Farms", () => new FarmerNatureCheck());
        RegisterContent("Tasks", () => new FarmerTasks());

        SideMenuControlType = typeof(ArlaEmployeePageSideMenuUC);
        SideMenuViewModelType = typeof(ArlaEmployeePageSideMenuUCViewModel);

        InitializeNavigation("Dashboards");
        SwitchContentView(CurrentNavigationTag);
    }
}
