using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
/// This page orchestrates the FarmerSideMenu and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class FarmerPage : Page
{
    public FarmerPageViewModel ViewModel { get; }

    public FarmerPage()
    {
        InitializeComponent();

        // Get dependencies from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();

        // Initialize ViewModel with required dependencies
        ViewModel = new FarmerPageViewModel(navigationHandler, personRepository, roleRepository);

        // Set DataContext so bindings work
        DataContext = ViewModel;

        // Let the ViewModel hook into the view's lifecycle events so it can attach/restore the SideMenu
        ViewModel.AttachToView(this);
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Role role)
        {
            await ViewModel.InitializeAsync(role);
        }

        // Set default view to Dashboards in ViewModel
        ViewModel.NavigationCommand?.Execute("Dashboards");

        // Attach SideMenu via ViewModel
        ViewModel.AttachSideMenuToMainWindow();
    }

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // Restore previous SideMenu children when navigating away via ViewModel
        ViewModel.RestoreMainWindowSideMenu();
    }
}

