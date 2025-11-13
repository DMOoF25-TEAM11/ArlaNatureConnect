using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.View.Pages.Farmer;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

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
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();
        
        ViewModel = new FarmerPageViewModel(navigationHandler, personRepository, roleRepository);
        DataContext = ViewModel;

        // Bind the page's ContentPresenter to the ViewModel's CurrentContent property when it changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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
    }

    private void FarmerPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Ensure the content presenter shows the current ViewModel content
        if (ContentPresenter.Content == null && ViewModel.CurrentContent != null)
        {
            ContentPresenter.Content = ViewModel.CurrentContent;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FarmerPageViewModel.CurrentContent))
        {
            ContentPresenter.Content = ViewModel.CurrentContent;
        }
    }
}

