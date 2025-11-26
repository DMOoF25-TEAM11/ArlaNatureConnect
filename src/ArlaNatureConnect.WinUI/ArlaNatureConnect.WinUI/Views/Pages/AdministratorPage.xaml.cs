using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.ViewModels.Pages;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdministratorPage : NavPage
{
    public AdministratorPage()
    {
        InitializeComponent();

        // Resolve the page view-model from DI so its dependencies (e.g. NavigationHandler) are injected.
        AdministratorPageViewModel vm = App.HostInstance.Services.GetRequiredService<AdministratorPageViewModel>();

        ViewModel = vm;      // required by NavPage
        DataContext = vm;    // bindings in XAML

        // Let the view-model attach to this page (subscribe to lifecycle events, attach side menu, etc.)
        //vm.AttachToView(this);

        // If a side-menu control exists in the visual tree and its DataContext is a SideMenuViewModelBase,
        // ensure it knows about this page view-model (backwards compatibility with existing side-menu wiring).
        FrameworkElement? sideMenuControl = FindName("SideMenu") as FrameworkElement;
        if (sideMenuControl?.DataContext is SideMenuViewModelBase sideMenuVm)
        {
            sideMenuVm.SetHostPageViewModel(vm);
        }
    }
}
