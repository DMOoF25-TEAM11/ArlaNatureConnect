using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.View.Pages.ArlaEmployee;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Arla Employee role users to view their dashboard directly.
/// This page orchestrates the ArlaEmployeeSideMenuUC and switches between different content views
/// (Dashboards, Farms, Users) based on navigation selection.
/// </summary>
public sealed partial class ArlaEmployeePage : Page
{
    public ArlaEmployeePageViewModel ViewModel { get; }

    public ArlaEmployeePage()
    {
        InitializeComponent();

        // Get NavigationHandler from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        ViewModel = new ArlaEmployeePageViewModel(navigationHandler);
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is Role role)
        {
            ViewModel.Initialize(role);
        }
        
        // Subscribe to CurrentNavigationTag property changes to update content view
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Set default view to Dashboards in ViewModel
        // Note: We don't call SwitchContentView here because ContentPresenter might not be ready yet
        // Instead, we'll load it in the Loaded event
        ViewModel.NavigationCommand?.Execute("Dashboards");
    }

    private void ArlaEmployeePage_Loaded(object sender, RoutedEventArgs e)
    {
        // Load default content view when page is fully loaded
        // This ensures ContentPresenter is ready to receive content
        string tag = ViewModel.CurrentNavigationTag;
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
        if (e.PropertyName == nameof(ArlaEmployeePageViewModel.CurrentNavigationTag))
        {
            // Switch the content view when navigation tag changes
            SwitchContentView(ViewModel.CurrentNavigationTag);
        }
    }

    /// <summary>
    /// Switches the content view based on the navigation tag.
    /// </summary>
    /// <param name="navigationTag">The tag of the selected navigation item (Dashboards, Farms, Users).</param>
    private void SwitchContentView(string navigationTag)
    {
        UserControl? newContent = null;

        switch (navigationTag)
        {
            case "Dashboards":
                newContent = new ArlaEmployeeDashboards();
                break;
            case "Farms":
                newContent = new ArlaEmployeeFarms();
                break;
            case "Users":
                newContent = new ArlaEmployeeUsers();
                break;
            default:
                // Default to Dashboards if unknown tag
                newContent = new ArlaEmployeeDashboards();
                break;
        }

        if (newContent != null)
        {
            newContent.DataContext = ViewModel;
            ContentPresenter.Content = newContent;
        }
    }
}

