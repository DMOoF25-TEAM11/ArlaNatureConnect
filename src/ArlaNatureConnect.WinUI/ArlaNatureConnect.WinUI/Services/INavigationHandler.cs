using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Services;

/// <summary>
/// Abstraction for frame-based navigation used by the WinUI application.
/// </summary>
/// <remarks>
/// Why we have it:
/// This interface decouples navigation consumers (view-models, services) from the concrete
/// navigation implementation. It makes the navigation surface testable, swappable and easy to
/// register in the DI container.
/// 
/// How to use:
/// - Register a concrete implementation in the DI container (for example <c>NavigationHandler</c>). 
/// - Initialize the implementation once with the application's main <c>Frame</c> (typically from the main window).
/// - Inject <c>INavigationHandler</c> into view-models or services and call <see cref="Navigate"/>, <see cref="GoBack"/>,
///   or inspect <see cref="CanGoBack"/> as needed.
/// </remarks>
/// <example>
/// Example (registration, initialization and usage):
/// <code language="csharp">
/// // In Program/Startup when configuring services
/// services.AddSingleton&lt;INavigationHandler, NavigationHandler&gt;();
/// 
/// // In MainWindow code-behind after frame has been created
/// var nav = App.HostInstance.Services.GetRequiredService&lt;INavigationHandler&gt;();
/// nav.Initialize(this.MainFrame);
/// 
/// // In a view-model or command
/// public class LoginViewModel
/// {
///     private readonly INavigationHandler _navigation;
///     public LoginViewModel(INavigationHandler navigation) { _navigation = navigation; }
///     public void OnLoginSuccess() => _navigation.Navigate(typeof(HomePage), userRole);
/// }
/// </code>
/// </example>
public interface INavigationHandler
{
    /// <summary>
    /// Initialize the navigation handler with the application frame. Must be called before navigation methods.
    /// </summary>
    /// <param name="frame">The Frame used for navigation.</param>
    // Inline note: call Initialize once (typically from the MainWindow) before calling Navigate/GoBack.
    void Initialize(Frame frame);

    /// <summary>
    /// Navigate to the specified page type with an optional parameter.
    /// </summary>
    /// <param name="pageType">The page <see cref="System.Type"/> to navigate to (e.g. typeof(MyPage)).</param>
    /// <param name="parameter">Optional parameter passed to the destination page.</param>
    void Navigate(Type pageType, object? parameter = null);

    /// <summary>
    /// Attempts to navigate back. Returns <c>true</c> if navigation occurred.
    /// </summary>
    /// <returns><c>true</c> when the frame was able to navigate back; otherwise <c>false</c>.</returns>
    bool GoBack();

    /// <summary>
    /// Indicates whether the frame can navigate back.
    /// </summary>
    bool CanGoBack { get; }
}
