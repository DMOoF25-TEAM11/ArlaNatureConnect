using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;

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


#if DEBUG
        // perform any async startup work here
        await Task.Delay(5000); // simulate initialization (5 seconds)
#else
        // perform any async startup work here
        await Task.Delay(800); // simulate initialization (0.8 seconds)
#endif



        // create and show main window
        _window = new MainWindow();
        _window.Activate();

        // close start window
        start.Close();
    }
}
