using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Services;

/// <summary>
/// Concrete frame-based navigation handler used by the WinUI application.
/// This class implements <see cref="INavigationHandler"/> and provides the actual
/// navigation behavior using a <see cref="Frame"/> instance.
/// </summary>
/// <remarks>
/// Why we have it:
/// This concrete implementation decouples navigation consumers (view-models, services) from
/// platform-specific navigation logic while still providing a simple API. By depending on
/// <see cref="INavigationHandler"/>, callers become testable and DI-friendly.
/// 
/// How to use:
/// - Register the implementation in the DI container (for example <c>NavigationHandler</c>).
/// - Initialize the implementation once with the application's main <see cref="Frame"/> (typically from the main window).
/// - Inject <see cref="INavigationHandler"/> into view-models or services and call <see cref="Navigate(Type, object?)"/>, <see cref="GoBack"/>,.
///   or inspect <see cref="CanGoBack"/> as needed.
/// 
/// Example:
/// <code language="csharp">
/// // Register in Program/Startup
/// services.AddSingleton&lt;INavigationHandler, NavigationHandler&gt;();
///
/// // Initialize (MainWindow code-behind)
/// var nav = App.HostInstance.Services.GetRequiredService&lt;INavigationHandler&gt;();
/// nav.Initialize(this.MainFrame);
///
/// // Usage in a view-model
/// public class LoginViewModel
/// {
///     private readonly INavigationHandler _navigation;
///     public LoginViewModel(INavigationHandler navigation) { _navigation = navigation; }
///     public void OnLoginSuccess() => _navigation.Navigate(typeof(HomePage), userRole);
/// }
/// </code>
/// </remarks>
public class NavigationHandler : INavigationHandler
{
    private Frame? _frame;

    /// <inheritdoc/>
    public void Initialize(Frame frame)
    {
        // Inline note: Initialize should be called once (typically from MainWindow) before any navigation.
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    /// <inheritdoc/>
    public virtual void Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("NavigationHandler has not been initialized. Call Initialize() first.");
        }

        _frame.Navigate(pageType, parameter);
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        if (_frame == null || !_frame.CanGoBack)
        {
            return false;
        }

        _frame.GoBack();
        return true;
    }

    /// <inheritdoc/>
    public bool CanGoBack => _frame?.CanGoBack ?? false;
}
