using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.Extensions.DependencyInjection;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Farmer role users to select a specific farmer and view their dashboard.
/// This page orchestrates the FarmerSideMenu and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class FarmerPage : NavPage
{
    public FarmerPage()
    {
        InitializeComponent();

        // Get dependencies from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();

        FarmerPageViewModel vm = new FarmerPageViewModel();
        ViewModel = vm;           // required by NavPage
        DataContext = vm;         // bindings in XAML
    }
}
