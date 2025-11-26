using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Administrator;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

public class AdministratorPageViewModel : NavigationViewModelBase
{
    public AdministratorPageViewModel(NavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        RegisterContent("Dashboards", () => new AdministratorDashboard());
        // Additional tags can be registered here when implemented.

        SideMenuControlType = typeof(AdministratorPageSideMenuUC);
        SideMenuViewModelType = typeof(AdministratorPageSideMenuUCViewModel);

        InitializeNavigation("Dashboards");
        SwitchContentView(CurrentNavigationTag);
    }
}