using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI.Controls;

public sealed partial class MenuBarUC : UserControl
{
    public MenuBarUC()
    {
        this.InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Exit();
    }

    private async void About_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ContentDialog
        {
            Title = "Om",
            Content = "Arla Nature Connect - Eksempel applikation",
            CloseButtonText = "Luk"
        };

        // ContentDialog must have a XamlRoot when shown from a UserControl in WinUI3
        dlg.XamlRoot = this.XamlRoot;

        await dlg.ShowAsync();
    }
}
