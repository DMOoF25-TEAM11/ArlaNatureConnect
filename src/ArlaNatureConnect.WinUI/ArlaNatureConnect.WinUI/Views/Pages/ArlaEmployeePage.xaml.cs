using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Arla Employee role users to view their dashboard directly.
/// This page orchestrates the ArlaEmployeeSideMenuUC and switches between different content views
/// (Dashboards, Farms, Users) based on navigation selection.
/// </summary>
public sealed partial class ArlaEmployeePage : Page
{
    public ArlaEmployeePageViewModel ViewModel { get; }

    public ArlaEmployeePage() : base()
    {
        InitializeComponent();

        // Get dependencies from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();

        // Initialize ViewModel with required dependencies
        ViewModel = new ArlaEmployeePageViewModel(navigationHandler, personRepository, roleRepository);

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

