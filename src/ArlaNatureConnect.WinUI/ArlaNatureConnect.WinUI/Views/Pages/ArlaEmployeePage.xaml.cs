using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// Page for Arla Employee role users to view their dashboard directly.
/// This page orchestrates the ArlaEmployeeSideMenuUC and switches between different content views
/// (Dashboards, Farms, Users) based on navigation selection.
/// </summary>
public sealed partial class ArlaEmployeePage : NavPage
{
    public ArlaEmployeePage() : base()
    {
        InitializeComponent();

        ArlaEmployeePageViewModel vm = App.HostInstance?.Services.GetService(typeof(ArlaEmployeePageViewModel)) as ArlaEmployeePageViewModel ?? throw new InvalidOperationException("Failed to resolve ArlaEmployeePageViewModel");

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
