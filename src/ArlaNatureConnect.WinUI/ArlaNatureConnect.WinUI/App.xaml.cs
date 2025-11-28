using ArlaNatureConnect.Infrastructure; // extension AddInfrastructure

using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Microsoft.Extensions.Hosting;

namespace ArlaNatureConnect.WinUI;

using ArlaNatureConnect.Core.Services;
using ArlaNatureConnect.WinUI.Services;
using ArlaNatureConnect.WinUI.ViewModels;
using ArlaNatureConnect.WinUI.ViewModels.Controls;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SharedUC;
using ArlaNatureConnect.WinUI.ViewModels.Controls.SideMenu;
using ArlaNatureConnect.WinUI.ViewModels.Pages;

using Microsoft.UI.Xaml;


/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    public static IHost HostInstance { get; private set; } = null!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // show start window first
        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddInfrastructure()

                    // Windows
                    .AddScoped<StartWindow>()
                    .AddScoped<StartWindowViewModel>()
                    .AddSingleton<MainWindow>()

                    // Services
                    .AddSingleton<NavigationHandler>()

                    // Controls
                    .AddSingleton<StatusBarUCViewModel>()

                    // Pages
                    .AddScoped<AdministratorPageViewModel>()
                    .AddScoped<ArlaEmployeePageViewModel>()
                    .AddScoped<ConsultantPageViewModel>()
                    .AddScoped<FarmerPageViewModel>()

                    // Feature ViewModels
                    .AddScoped<ArlaEmployeeAssignNatureCheckViewModel>()

                    // Side Menus
                    .AddScoped<AdministratorPageSideMenuUCViewModel>()
                    .AddScoped<ArlaEmployeePageSideMenuUCViewModel>()
                    .AddScoped<ConsultantPageSideMenuUCViewModel>()
                    .AddScoped<FarmerPageSideMenuUCViewModel>()

                    // Shared Controls
                    .AddScoped<CRUDPersonUCViewModel>()
                    ;

            })
            .Build();

        // Resolve IConnectionStringService from DI
        IConnectionStringService connService = HostInstance.Services.GetRequiredService<IConnectionStringService>();

        StartWindow start = HostInstance.Services.GetRequiredService<StartWindow>();
        start.Activate();

        Task[] taskInit =
        [
            Task.Delay(400),       // simulate initialization
            start.Initialization,  // task from StartWindow
        ];
        await Task.WhenAll(taskInit);

        // resolve and show main window from DI so its dependencies are injected
        _window = HostInstance.Services.GetRequiredService<MainWindow>();
        _window.Activate();

        // close start window
        start.Close();
    }

    #region Helpers
    /// <summary>
    /// Helper to expose the main window's XamlRoot so non-UI code can attach dialogs to the visual tree.
    /// Returns null if the main window isn't available yet.
    /// </summary>
    public static XamlRoot? MainWindowXamlRoot
    {
        get
        {
            try
            {
                // If HostInstance is built and MainWindow is registered as a singleton, resolve it and return its Content.XamlRoot
                MainWindow? mainWindow = HostInstance?.Services.GetService<MainWindow>();
                if (mainWindow?.Content is FrameworkElement fe)
                {
                    return fe.XamlRoot;
                }
            }
            catch
            {
                // ignore resolution failures
            }

            return null;
        }
    }
    #endregion
}
