using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu; // for FarmerSideMenuUCViewModel
using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml; // Ensure this is included for RoutedEventArgs

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Farmer sidebar navigation menu.
/// View-specific logic is minimal; common behavior is provided by UCSideMenu base defined in XAML.
/// </summary>
public sealed partial class FarmerPageSideMenuUC : SideMenuBaseUC
{
    public FarmerPageSideMenuUC()
    {
        InitializeComponent();
        Loaded += FarmerPageSideMenuUC_Loaded;
    }

    private async void FarmerPageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        // At runtime, ensure the view-model initializes and loads persons.
        // Design-time DataContext is set via d:DataContext in XAML; at runtime it comes from the parent/page.
        if (DataContext is FarmerPageSideMenuUCViewModel vm)
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
