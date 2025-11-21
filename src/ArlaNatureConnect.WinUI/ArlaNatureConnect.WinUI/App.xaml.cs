using ArlaNatureConnect.Infrastructure; // extension AddInfrastructure

using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Microsoft.Extensions.Hosting;

namespace ArlaNatureConnect.WinUI;

using ArlaNatureConnect.Core;
using ArlaNatureConnect.WinUI.Services;

using Microsoft.UI.Xaml;


/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    public static IHost HostInstance { get; private set; } = null!;

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
        StartWindow start = new StartWindow();
        start.Activate();

        Task delayTask = Task.Delay(800); // simulate initialization (0.8 seconds)
        Task initTask = start.Initialization; // task that completes when start window initialization (including any dialog) is done

        await Task.WhenAll(initTask, delayTask);

        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    //.AddCoreServices()
                    .AddInfrastructure()
                    .AddSingleton<NavigationHandler>()
                    .AddSingleton<MainWindow>()
                    .AddSingleton<ArlaNatureConnect.WinUI.ViewModels.Controls.StatusBarUCViewModel>()
                    ;

                // register viewmodels used by controls so controls can resolve a runtime DataContext
                //.AddSingleton<ArlaNatureConnect.WinUI.ViewModels.Controls.StatusBarUCViewModel>();
            })
            .Build();

        // resolve and show main window from DI so its dependencies are injected
        _window = HostInstance.Services.GetRequiredService<MainWindow>();
        _window.Activate();

        // close start window
        start.Close();
    }
}
