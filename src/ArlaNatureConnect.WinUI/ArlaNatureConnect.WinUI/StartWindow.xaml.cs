using System;
using Microsoft.UI.Xaml;

namespace ArlaNatureConnect.WinUI;

public sealed partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();

        // Attach to the root content Loaded event if available
        if (this.Content is FrameworkElement fe)
        {
            fe.Loaded += StartWindow_Loaded;
        }
        else
        {
            // Fallback: call the async loader directly
            StartWindow_Loaded(null, null);
        }
    }

    private async void StartWindow_Loaded(object? sender, RoutedEventArgs? e)
    {
        try
        {
            // Try packaged-app URI first (works only if app is packaged)
            var uri = new Uri("ms-appx:///Assets/startUpImage.jpg");
            try
            {
                var f = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                System.Diagnostics.Debug.WriteLine($"Found (packaged): {f.Path}");
            }
            catch
            {
                // ignore and try file system fallback
            }

            // Fallback for unpackaged / desktop scenarios: check output folder
            var filePath = System.IO.Path.Combine(AppContext.BaseDirectory ?? string.Empty, "Assets", "startUpImage.jpg");
            if (System.IO.File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Found (fs): {filePath}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Image not found at either ms-appx or output folder: {filePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading startup image: {ex}");
        }
    }
}