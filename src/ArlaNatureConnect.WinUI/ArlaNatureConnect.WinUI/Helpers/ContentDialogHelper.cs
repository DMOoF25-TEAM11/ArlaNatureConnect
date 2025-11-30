using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Helpers;

/// <summary>
/// Provides helper methods for displaying common content dialogs.
/// </summary>
public static class ContentDialogHelper
{
    /// <summary>
    /// Shows a confirmation dialog with primary/close buttons.
    /// </summary>
    /// <param name="xamlRoot">The XamlRoot to host the dialog.</param>
    /// <param name="title">Dialog title.</param>
    /// <param name="content">Dialog content.</param>
    /// <param name="primaryButtonText">Primary button text.</param>
    /// <param name="closeButtonText">Close button text.</param>
    /// <returns><c>true</c> if the primary button was clicked; otherwise <c>false</c>.</returns>
    public static async Task<bool> ShowConfirmationAsync(
        XamlRoot? xamlRoot,
        string title,
        string content,
        string primaryButtonText = "OK",
        string closeButtonText = "Annuller")
    {
        if (xamlRoot == null)
        {
            return false;
        }

        ContentDialog dialog = new()
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText,
            XamlRoot = xamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public static async Task ShowInfoAsync(
        XamlRoot? xamlRoot,
        string title,
        string content,
        string closeButtonText = "OK")
    {
        if (xamlRoot == null)
        {
            return;
        }

        ContentDialog dialog = new()
        {
            Title = title,
            Content = content,
            CloseButtonText = closeButtonText,
            XamlRoot = xamlRoot
        };

        await dialog.ShowAsync();
    }
}

