using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

public sealed partial class AdministratorPageSideMenuUC : SideMenuBaseUC
{
    public AdministratorPageSideMenuUC()
    {
        InitializeComponent();
        Loaded += AdministratorPageSideMenuUC_Loaded;
    }

    private void AdministratorPageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }
}
