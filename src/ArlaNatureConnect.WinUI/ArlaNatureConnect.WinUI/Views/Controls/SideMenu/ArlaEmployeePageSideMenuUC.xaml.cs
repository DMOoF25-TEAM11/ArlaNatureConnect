using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI.Views.Controls.SideMenu;

/// <summary>
/// UserControl for the Arla Employee sidebar navigation menu.
/// Handles navigation buttons.
/// </summary>
public sealed partial class ArlaEmployeePageSideMenuUC : SideMenuBaseUC
{
    public ArlaEmployeePageSideMenuUC()
    {
        InitializeComponent();
        Loaded += ArlaEmployeePageSideMenuUC_Loaded;
    }

    private void ArlaEmployeePageSideMenuUC_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonStyles();
    }
}

