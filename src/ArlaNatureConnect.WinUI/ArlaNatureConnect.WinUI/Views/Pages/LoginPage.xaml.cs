using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Login page for role selection.
/// </summary>
public sealed partial class LoginPage : Page
{
    //public LoginPageViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();

        // Get NavigationHandler from App's service provider
        //NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<ArlaNatureConnect.WinUI.Services.NavigationHandler>();
        //ViewModel = new LoginPageViewModel(navigationHandler);
        //DataContext = ViewModel;
    }
}

