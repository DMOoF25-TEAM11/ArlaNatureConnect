using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Consultant role users to select a specific consultant and view their dashboard.
/// This page orchestrates the ConsultantSidebar and switches between different content views
/// (Dashboards, Nature Check, Tasks) based on navigation selection.
/// </summary>
public sealed partial class ConsultantPage : NavPage
{
    public ConsultantPage()
    {
        InitializeComponent();

        // Resolve the page view-model from DI so its dependencies (e.g. NavigationHandler) are injected.
        ConsultantPageViewModel vm = App.HostInstance.Services.GetRequiredService<ConsultantPageViewModel>();

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

