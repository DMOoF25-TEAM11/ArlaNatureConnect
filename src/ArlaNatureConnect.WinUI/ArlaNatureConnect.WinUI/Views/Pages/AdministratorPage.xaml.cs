using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.Views.Pages.Abstracts;

using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdministratorPage : NavPage
{
    public AdministratorPage()
    {
        InitializeComponent();

        // Get dependencies from App's service provider
        NavigationHandler navigationHandler = App.HostInstance.Services.GetRequiredService<NavigationHandler>();
        IPersonRepository personRepository = App.HostInstance.Services.GetRequiredService<IPersonRepository>();
        IRoleRepository roleRepository = App.HostInstance.Services.GetRequiredService<IRoleRepository>();

    }
}
