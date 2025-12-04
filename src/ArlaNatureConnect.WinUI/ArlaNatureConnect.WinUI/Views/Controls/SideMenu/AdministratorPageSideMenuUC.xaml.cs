using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

public sealed partial class AdministratorPageSideMenuUC : SideMenuBaseUC
{
    public AdministratorPageSideMenuUC()
    {
        InitializeComponent();
        Loaded += AdministratorPageSideMenuUC_Loaded;
        DataContext = App.HostInstance?.Services.GetService(typeof(AdministratorPageSideMenuUCViewModel)) as AdministratorPageSideMenuUCViewModel;
    }

    private void AdministratorPageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        // Set DataContext from DI container
        UpdateButtonStyles();
    }
}
