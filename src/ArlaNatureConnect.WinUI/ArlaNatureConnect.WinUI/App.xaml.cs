using ArlaNatureConnect.Infrastructure; // extension AddInfrastructure

using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Microsoft.Extensions.Hosting;

namespace ArlaNatureConnect.WinUI;

using ArlaNatureConnect.Core;

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
        StartWindow start = new StartWindow();
        start.Activate();

        Task delayTask = Task.Delay(800); // simulate initialization (0.8 seconds)
        Task initTask = start.Initialization; // task that completes when start window initialization (including any dialog) is done

        await Task.WhenAll(initTask, delayTask);

        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddCoreServices()
                    .AddInfrastructure()
                    .AddSingleton<Services.NavigationHandler>()
                    .AddSingleton<MainWindow>();

                // register viewmodels used by controls
                //services.AddTransient<ArlaNatureConnect.WinUI.ViewModels.Controls.StatusBarUCViewModel>();
            })
            .Build();

        // resolve and show main window from DI so its dependencies are injected
        _window = HostInstance.Services.GetRequiredService<MainWindow>();
        _window.Activate();

        // close start window
        start.Close();
    }
}
