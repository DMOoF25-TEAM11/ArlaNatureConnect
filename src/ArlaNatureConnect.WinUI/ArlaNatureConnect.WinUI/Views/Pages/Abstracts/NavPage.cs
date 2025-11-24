using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

public abstract partial class NavPage : Page
{

    public INavigationViewModel? ViewModel { get; set; }
    public string DefaultNavigationViewItemTag { get; set; } = "Dashboards";

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (ViewModel is null)
        {
            throw new InvalidOperationException("ViewModel must be set before navigating to NavPage.");
        }

        if (e.Parameter is Role role)
        {
            if (role is null)
            {
                throw new ArgumentNullException(nameof(role), "Role parameter cannot be null when navigating to NavPage.");
            }
            await ViewModel.InitializeAsync(role);
        }

        // Set default view to Dashboards in ViewModel
        ViewModel?.NavigationCommand?.Execute(DefaultNavigationViewItemTag);

        // Attach SideMenu via ViewModel
        ViewModel?.AttachSideMenuToMainWindow();
    }


    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Restore previous SideMenu children when navigating away via ViewModel
        ViewModel?.RestoreMainWindowSideMenu();
    }
}
