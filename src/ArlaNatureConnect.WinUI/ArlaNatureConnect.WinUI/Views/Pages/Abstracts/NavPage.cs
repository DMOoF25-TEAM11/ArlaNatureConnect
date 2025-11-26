using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

/// <summary>
/// Base page that hosts a navigation-enabled view-model and wires common navigation lifecycle behavior.
/// 
/// Why this class exists:
/// - Centralizes the common setup performed when navigating to role-based pages: validating the view-model,
///   invoking its asynchronous initializer with the current <see cref="Role"/>, selecting the default
///   navigation item and attaching the side menu to the main window.
/// - Keeps page classes small by moving shared navigation/side-menu wiring to a single abstract base.
/// - Ensures consistent behavior (initialization, default selection, side-menu attach/restore) across all
///   pages that present role-scoped dashboards or content.
/// </summary>
public abstract partial class NavPage : Page
{

    /// <summary>
    /// The view-model the page hosts. Must implement <see cref="INavigationViewModelBase"/> and be set before navigating
    /// to this page. The page will call <see cref="INavigationViewModelBase.InitializeAsync(Role?)"/> during navigation.
    /// </summary>
    public INavigationViewModelBase? ViewModel { get; set; }

    /// <summary>
    /// Called when the page is navigated to. This implementation:
    /// - Validates that <see cref="ViewModel"/> is set and throws <see cref="InvalidOperationException"/> otherwise.
    /// - If the navigation parameter is a <see cref="Role"/>, invokes <see cref="INavigationViewModelBase.InitializeAsync"/>.
    /// - Executes the view-model's <see cref="INavigationViewModelBase.NavigationCommand"/> with
    ///   <see cref="DefaultNavigationViewItemTag"/> to select the default content.
    /// - Attaches the view-model's side menu to the main window via <see cref="INavigationViewModelBase.AttachSideMenuToMainWindow"/>.
    /// 
    /// Derived pages may override but should call <c>base.OnNavigatedTo(e)</c> to preserve this common behavior.
    /// </summary>
    /// <param name="e">Navigation event arguments provided by the framework.</param>
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

        // Attach SideMenu via ViewModel
        ViewModel?.AttachSideMenuToMainWindow();
    }


    /// <summary>
    /// Called when the page is navigated away from. Restores the application's main window side menu to its previous
    /// children by delegating to <see cref="INavigationViewModelBase.RestoreMainWindowSideMenu"/> on the view-model.
    /// Derived pages should call <c>base.OnNavigatedFrom(e)</c> to ensure the side-menu is restored.
    /// </summary>
    /// <param name="e">Navigation event arguments provided by the framework.</param>
    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
    }
}
