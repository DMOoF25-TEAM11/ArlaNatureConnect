using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Consultant role users to select a specific consultant and view their dashboard.
/// </summary>
public sealed partial class ConsultantPage : Page
{
    public ConsultantPageViewModel ViewModel { get; }

    public ConsultantPage()
    {
        InitializeComponent();
        
        // Get dependencies from App's service provider
        var navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        var personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        var roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();
        
        ViewModel = new ConsultantPageViewModel(navigationHandler, personRepository, roleRepository);
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Role role)
        {
            await ViewModel.InitializeAsync(role);
        }
    }

    private void ConsultantPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Set default selected button to "Gårde og Natur Check" after page is loaded
        if (FarmsButton != null && Application.Current.Resources.TryGetValue("ArlaPrimaryButton", out var primaryStyle))
        {
            // Highlight the default button
            FarmsButton.Style = primaryStyle as Microsoft.UI.Xaml.Style;
        }
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            // Get styles from application resources
            if (Application.Current.Resources.TryGetValue("ArlaSecondaryButton", out var secondaryStyle) &&
                Application.Current.Resources.TryGetValue("ArlaPrimaryButton", out var primaryStyle))
            {
                var secondaryStyleTyped = secondaryStyle as Microsoft.UI.Xaml.Style;
                var primaryStyleTyped = primaryStyle as Microsoft.UI.Xaml.Style;
                
                // Reset all buttons to secondary style
                DashboardsButton.Style = secondaryStyleTyped;
                FarmsButton.Style = secondaryStyleTyped;
                TasksButton.Style = secondaryStyleTyped;
                
                // Set clicked button to primary style
                button.Style = primaryStyleTyped;
            }
            
            // Handle navigation
            ViewModel.OnNavigationItemSelected(tag);
        }
    }
}

