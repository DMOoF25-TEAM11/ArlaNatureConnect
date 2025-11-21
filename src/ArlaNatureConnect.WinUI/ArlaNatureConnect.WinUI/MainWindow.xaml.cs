using ArlaNatureConnect.WinUI.View.Pages;
using ArlaNatureConnect.WinUI.Services; 
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WinRT.Interop;
using Microsoft.UI;

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
        Activated += MainWindow_Activated;
        
        // Initialize navigation handler with the content frame
        _navigationHandler.Initialize(ContentFrame);
        
        // Navigate to login page on startup
        _navigationHandler.Navigate(typeof(LoginPage));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Set window size
        nint windowHandle = WindowNative.GetWindowHandle(this);
        WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow != null)
        {
            SizeInt32 size = new(1300, 1000);
            appWindow.Resize(size);

            try
            {
                // Center the window in the display work area
                DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                if (displayArea != null)
                {
                    RectInt32 work = displayArea.WorkArea;
                    int centerX = work.X + (work.Width - size.Width) / 2;
                    int centerY = work.Y + (work.Height - size.Height) / 2;

                    PointInt32 position = new(centerX, centerY);
                    appWindow.Move(position);
                }
            }
            catch
            {
                // If centering fails for any reason, ignore and keep resized position
            }
        }
        
        // Unsubscribe after first activation
        Activated -= MainWindow_Activated;
    }
}
