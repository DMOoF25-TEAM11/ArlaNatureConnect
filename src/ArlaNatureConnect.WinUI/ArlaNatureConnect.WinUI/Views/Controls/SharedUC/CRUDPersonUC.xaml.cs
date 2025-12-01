using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.SharedUC;

public sealed partial class CRUDPersonUC : UserControl
{
    public CRUDPersonUC()
    {
        InitializeComponent();

        DataContext = App.HostInstance?.Services.GetRequiredService<CRUDPersonUCViewModel>()
            ?? throw new InvalidOperationException("Application host not initialized or CRUDPersonUCViewModel not registered in DI.");
    }

    private void Button_GettingFocus(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.Input.GettingFocusEventArgs args)
    {

    }
}
