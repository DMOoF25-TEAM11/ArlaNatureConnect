using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Login page for role selection.
/// </summary>
public sealed partial class LoginPage : Page
{
    public LoginPageViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();

        // Get NavigationHandler from App's service provider
        NavigationHandler navigationHandler = (NavigationHandler)App.HostInstance.Services.GetRequiredService<ArlaNatureConnect.WinUI.Services.INavigationHandler>();
        ViewModel = new LoginPageViewModel(navigationHandler);
        DataContext = ViewModel;
    }
}

