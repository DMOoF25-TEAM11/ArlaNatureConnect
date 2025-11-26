using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for ConsultantPage - handles user selection, navigation and dashboard display for consultants.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via InitializeAsync()
/// - Loads all available persons with Consultant role from repositories
/// - Filters persons based on role and active status
/// - Sorts persons alphabetically (first name, last name)
/// - Handles user selection from dropdown menu
/// - Handles navigation between different sections (Dashboards, Farms and Nature Check, My Tasks)
/// - Loads dashboard for the selected consultant
/// 
/// Usage:
/// This ViewModel is used by ConsultantPage.xaml to display a dropdown with all consultants,
/// navigation between different sections, and when a consultant is selected, their dashboard is displayed.
/// The ViewModel also handles loading state to show progress indicator while data is loading.
/// </summary>
public class ConsultantPageViewModel : NavigationViewModelBase
{
    public ConsultantPageViewModel(NavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        //RegisterContent("Dashboards", () => new AdministratorDashboard());

        SideMenuControlType = typeof(ConsultantPageSideMenuUC);
        SideMenuViewModelType = typeof(ConsultantPageSideMenuUCViewModel);

        //InitializeNavigation("Dashboards");
        //SwitchContentView(CurrentNavigationTag);
    }
}
