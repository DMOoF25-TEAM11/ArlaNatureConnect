using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI.Views.Controls.SharedUC;

public sealed partial class CRUDPersonUC : UserControl
{
    public CRUDPersonUC()
    {
        InitializeComponent();

        CRUDPersonUCViewModel vm = App.HostInstance.Services.GetRequiredService<CRUDPersonUCViewModel>();
        DataContext = vm;
    }
}
