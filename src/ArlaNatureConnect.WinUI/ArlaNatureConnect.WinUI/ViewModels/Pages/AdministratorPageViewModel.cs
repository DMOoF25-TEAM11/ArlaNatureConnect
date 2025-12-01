using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.PageContents.Administrator;
using ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

namespace ArlaNatureConnect.WinUI.ViewModels.Pages;

public class AdministratorPageViewModel : NavigationViewModelBase
{
    public AdministratorPageViewModel(INavigationHandler navigationHandler)
        : base(navigationHandler)
    {
        SideMenuControlType = typeof(AdministratorPageSideMenuUC);
        SideMenuViewModelType = typeof(AdministratorPageSideMenuUCViewModel);

        NavigateToView(() => new AdministratorDashboardUC());
    }
}