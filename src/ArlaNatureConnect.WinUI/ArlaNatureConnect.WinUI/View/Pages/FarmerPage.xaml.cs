using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.View.Pages.Farmer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
/// This page orchestrates the FarmerSidebar and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class FarmerPage : Page
{
    public FarmerPageViewModel ViewModel { get; }

    public FarmerPage()
    {
        InitializeComponent();
        
        // Get dependencies from App's service provider
        var navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        var personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        var roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();
        
        ViewModel = new FarmerPageViewModel(navigationHandler, personRepository, roleRepository);
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Role role)
        {
            await ViewModel.InitializeAsync(role);
        }
        
        // Subscribe to CurrentNavigationTag property changes to update content view
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Set default view to Dashboards in ViewModel
        // Note: We don't call SwitchContentView here because ContentPresenter might not be ready yet
        // Instead, we'll load it in the Loaded event
        ViewModel.NavigationCommand?.Execute("Dashboards");
    }

    private void FarmerPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Load default content view when page is fully loaded
        // This ensures ContentPresenter is ready to receive content
        var tag = ViewModel.CurrentNavigationTag;
        if (string.IsNullOrEmpty(tag))
        {
            tag = "Dashboards"; // Default fallback
            ViewModel.NavigationCommand?.Execute(tag);
        }
        
        // Always load the content view in Loaded event to ensure it's displayed
        SwitchContentView(tag);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FarmerPageViewModel.CurrentNavigationTag))
        {
            // Switch the content view when navigation tag changes
            SwitchContentView(ViewModel.CurrentNavigationTag);
        }
    }

    /// <summary>
    /// Switches the content view based on the navigation tag.
    /// </summary>
    /// <param name="navigationTag">The tag of the selected navigation item (Dashboards, Farms, Tasks).</param>
    private void SwitchContentView(string navigationTag)
    {
        UserControl? newContent = null;

        switch (navigationTag)
        {
            case "Dashboards":
                newContent = new FarmerDashboards();
                break;
            case "Farms":
                newContent = new FarmerNatureCheck();
                break;
            case "Tasks":
                newContent = new FarmerTasks();
                break;
            default:
                // Default to Dashboards if unknown tag
                newContent = new FarmerDashboards();
                break;
        }

        if (newContent != null)
        {
            newContent.DataContext = ViewModel;
            ContentPresenter.Content = newContent;
        }
    }
}

