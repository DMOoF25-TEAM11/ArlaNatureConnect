using ArlaNatureConnect.WinUI.View.Pages;
using ArlaNatureConnect.WinUI.Services; 
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArlaNatureConnect.WinUI;

/// <summary>
/// Main window for the application.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly NavigationHandler _navigationHandler;

    public MainWindow(NavigationHandler navigationHandler)
    {
        InitializeComponent();
        _navigationHandler = navigationHandler ?? throw new ArgumentNullException(nameof(navigationHandler));
        
        // Set window size after window is activated
        this.Activated += MainWindow_Activated;
        
        // Initialize navigation handler with the content frame
        _navigationHandler.Initialize(ContentFrame);
        
        // Navigate to login page on startup
        _navigationHandler.Navigate(typeof(LoginPage));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Set window size
        var windowHandle = WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow != null)
        {
            var size = new SizeInt32(1300, 1000);
            appWindow.Resize(size);
        }
        
        // Unsubscribe after first activation
        this.Activated -= MainWindow_Activated;
    }
}
