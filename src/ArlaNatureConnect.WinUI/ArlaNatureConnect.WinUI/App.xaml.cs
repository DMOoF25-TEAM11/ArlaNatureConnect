using ArlaNatureConnect.Infrastructure; // extension AddInfrastructure

using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Microsoft.Extensions.Hosting;

namespace ArlaNatureConnect.WinUI;

using ArlaNatureConnect.Core.Services;
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
                    .AddSingleton<NavigationHandler>()
                    .AddSingleton<MainWindow>()
                    .AddSingleton<ArlaNatureConnect.WinUI.ViewModels.Controls.StatusBarUCViewModel>()
                    ;
            })
            .Build();

        // Resolve IConnectionStringService from DI
        IConnectionStringService connService = HostInstance.Services.GetRequiredService<IConnectionStringService>();

        StartWindow start = new();
        start.Activate();

        Task delayTask = Task.Delay(400); // simulate initialization (0.4 seconds)
        Task initTask = start.Initialization; // task that completes when start window initialization (including any dialog) is done

        await Task.WhenAll(initTask, delayTask);

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
