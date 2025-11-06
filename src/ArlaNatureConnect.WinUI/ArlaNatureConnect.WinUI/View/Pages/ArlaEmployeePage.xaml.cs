using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages;

/// <summary>
/// Page for Arla Employee role users to view their dashboard directly.
/// </summary>
public sealed partial class ArlaEmployeePage : Page
{
    public ArlaEmployeePageViewModel ViewModel { get; }

    public ArlaEmployeePage()
    {
        InitializeComponent();
        
        // Get NavigationHandler from App's service provider
        var navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
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
    }
}

