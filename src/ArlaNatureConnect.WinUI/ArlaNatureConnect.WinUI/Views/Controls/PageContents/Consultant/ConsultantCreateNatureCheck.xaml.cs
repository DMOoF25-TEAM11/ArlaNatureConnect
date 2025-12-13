using ArlaNatureConnect.WinUI.ViewModels.Controls;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.PageContents.Consultant;

public sealed partial class ConsultantCreateNatureCheck : UserControl
{
    public ConsultantCreateNatureCheck()
    {
        InitializeComponent();
        DataContext = App.HostInstance.Services.GetRequiredService<ConsultantCreateNatureCheckViewModel>();
    }
}
