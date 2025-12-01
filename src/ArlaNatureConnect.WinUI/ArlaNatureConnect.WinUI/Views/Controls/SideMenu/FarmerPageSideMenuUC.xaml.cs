using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu; // for FarmerSideMenuUCViewModel
using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml; // Ensure this is included for RoutedEventArgs

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Farmer sidebar navigation menu.
/// View-specific logic is minimal; common behavior is provided by UCSideMenu base defined in XAML.
/// </summary>
public sealed partial class FarmerPageSideMenuUC : SideMenuBaseUC
{
    private IServiceScope? _sideMenuScope;

    public FarmerPageSideMenuUC()
    {
        InitializeComponent();
        Loaded += FarmerPageSideMenuUC_Loaded;
        Unloaded += FarmerPageSideMenuUC_Unloaded;
    }

    private void FarmerPageSideMenuUC_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _sideMenuScope?.Dispose();
            _sideMenuScope = null;
        }
        catch { }
    }

    private async void FarmerPageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        // At runtime, ensure the view-model initializes and loads persons.
        // Design-time DataContext is set via d:DataContext in XAML; at runtime it comes from the parent/page.
        FarmerPageSideMenuUCViewModel? vm = DataContext as FarmerPageSideMenuUCViewModel;

        // If the DataContext is not the expected side-menu VM, try resolving it from DI and assign it so bindings work.
        if (vm == null)
        {
            try
            {
                // Create a scope and resolve the scoped view-model so its scoped dependencies are honored.
                _sideMenuScope?.Dispose();
                _sideMenuScope = App.HostInstance?.Services.CreateScope();
                vm = _sideMenuScope?.ServiceProvider.GetService<FarmerPageSideMenuUCViewModel>();
                if (vm != null)
                {
                    DataContext = vm;
                }
            }
            catch
            {
                // swallow - resolution is best-effort
            }
        }

        if (vm != null)
        {
            // Optionally reflect loading state in the UI.
            vm.IsLoading = true;
            try
            {
                await vm.InitializeAsync(); // populates AvailablePersons
            }
            finally
            {
                vm.IsLoading = false;
            }
        }
        else
        {
            // If DataContext is not the expected VM, there's likely no runtime binding/injection.
            // In that case, AvailablePersons will be empty and ComboBox won't show items.
        }
    }
}
