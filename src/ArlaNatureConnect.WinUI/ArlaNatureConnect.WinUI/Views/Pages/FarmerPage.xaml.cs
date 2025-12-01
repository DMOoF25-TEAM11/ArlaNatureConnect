using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.UI.Xaml;

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

        // Resolve the page view-model from DI so its dependencies (e.g. NavigationHandler) are injected.
        FarmerPageViewModel vm = App.HostInstance?.Services.GetService(typeof(FarmerPageViewModel)) as FarmerPageViewModel ?? throw new InvalidOperationException("Failed to resolve FarmerPageViewModel");

        ViewModel = vm;      // required by NavPage
        DataContext = vm;    // bindings in XAML

        // If a side-menu control exists in the visual tree and its DataContext is a SideMenuViewModelBase,
        // ensure it knows about this page view-model (backwards compatibility with existing side-menu wiring).
        FrameworkElement? sideMenuControl = FindName("SideMenu") as FrameworkElement;
        if (sideMenuControl?.DataContext is SideMenuViewModelBase sideMenuVm)
        {
            sideMenuVm.SetHostPageViewModel(vm);
        }
    }
}
