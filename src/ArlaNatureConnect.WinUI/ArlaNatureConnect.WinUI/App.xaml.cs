using ArlaNatureConnect.Core; // extension AddCoreServices
using ArlaNatureConnect.Infrastructure; // extension AddInfrastructure
using Microsoft.Extensions.DependencyInjection; // Add this using directive
using Microsoft.Extensions.Hosting;

namespace ArlaNatureConnect.WinUI;

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
        var start = new StartWindow();
        start.Activate();

        var delayTask = Task.Delay(800); // simulate initialization (0.8 seconds)
        var initTask = start.Initialization; // task that completes when start window initialization (including any dialog) is done

        await Task.WhenAll(initTask, delayTask);

        HostInstance = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services
                    .AddCoreServices()
                    .AddInfrastructure()
                    .AddSingleton<MainWindow>();
            })
            .Build();

        // resolve and show main window from DI so its dependencies are injected
        _window = HostInstance.Services.GetRequiredService<MainWindow>();
        _window.Activate();

        // close start window
        start.Close();
    }
}
