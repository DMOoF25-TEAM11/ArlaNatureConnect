using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Services;

/// <summary>
/// Handles navigation between pages in the application.
/// </summary>
public class NavigationHandler
{
    private Frame? _frame;

    /// <summary>
    /// Initializes the navigation handler with the main content frame.
    /// </summary>
    /// <param name="frame">The Frame control used for navigation.</param>
    public void Initialize(Frame frame)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    /// <summary>
    /// Navigates to the specified page type with an optional parameter.
    /// </summary>
    /// <param name="pageType">The type of page to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the page.</param>
    public void Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("NavigationHandler has not been initialized. Call Initialize() first.");
        }

        _frame.Navigate(pageType, parameter);
    }

    /// <summary>
    /// Navigates back if possible.
    /// </summary>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    public bool GoBack()
    {
        if (_frame == null || !_frame.CanGoBack)
        {
            return false;
        }

        _frame.GoBack();
        return true;
    }

    /// <summary>
    /// Checks if navigation back is possible.
    /// </summary>
    public bool CanGoBack => _frame?.CanGoBack ?? false;
}


