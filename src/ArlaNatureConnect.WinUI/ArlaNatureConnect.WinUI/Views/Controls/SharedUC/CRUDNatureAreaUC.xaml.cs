using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI.Views.Controls.SharedUC;

public sealed partial class CRUDNatureAreaUC : UserControl
{
    public CRUDNatureAreaUC()
    {
        InitializeComponent();
        DataContext = App.HostInstance?.Services.GetService(typeof(CRUDNatureAreaUCViewModel)) as CRUDNatureAreaUCViewModel ?? throw new InvalidOperationException("Application host not initialized or CRUDNatureAreaUCViewModel not registered in DI.");
    }
}
