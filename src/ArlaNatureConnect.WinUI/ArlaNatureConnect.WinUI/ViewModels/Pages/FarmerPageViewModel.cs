using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Farmer;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

/// <summary>
/// ViewModel for FarmerPage - handles user selection and dashboard display for farmers.
/// 
/// Responsibilities:
/// - Receives Role object from LoginPage via InitializeAsync()
/// - Loads all available persons with Farmer role from repositories
/// - Filters persons based on role and active status
/// - Sorts persons alphabetically (first name, last name)
/// - Handles user selection from dropdown menu
/// - Loads dashboard for the selected farmer
/// 
/// Usage:
/// This ViewModel is used by FarmerPage.xaml to display a dropdown with all farmers,
/// and when a farmer is selected, their dashboard is displayed. The ViewModel also handles loading state
/// to show progress indicator while data is loading.
/// </summary>
public partial class FarmerPageViewModel : NavigationViewModelBase
{
    public FarmerPageViewModel(INavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        SideMenuControlType = typeof(FarmerPageSideMenuUC);
        SideMenuViewModelType = typeof(FarmerPageSideMenuUCViewModel);

        NavigateToView(() => new FarmerDashboards());
    }
}
