using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Login page for role selection.
/// </summary>
public sealed partial class LoginPage : Page
{
    // Parameterless ctor for XAML activation
    public LoginPage()
    {
        InitializeComponent();

        // Create view model with dependencies
        LoginPageViewModel vm = App.HostInstance?.Services.GetService(typeof(LoginPageViewModel)) as LoginPageViewModel ?? throw new InvalidOperationException("Failed to resolve LoginPageViewModel");
        DataContext = vm;
    }
}

