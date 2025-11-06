using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
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
    }
}

