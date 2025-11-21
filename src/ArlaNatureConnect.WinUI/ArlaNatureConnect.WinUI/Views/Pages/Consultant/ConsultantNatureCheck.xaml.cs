using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.View.Pages.Consultant;

/// <summary>
/// UserControl for the Consultant Nature Check view.
/// Contains search bar, action buttons, and data table for farms and nature checks.
/// </summary>
public sealed partial class ConsultantNatureCheck : UserControl
{
    public ConsultantNatureCheck()
    {
        InitializeComponent();
        Loaded += ConsultantNatureCheck_Loaded;
    }

    private void ConsultantNatureCheck_Loaded(object sender, RoutedEventArgs e)
    {
        // UI polish: Soft shadow (DropShadow) behind main card and search field
        if (TableCard != null)
        {
            var shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
            TableCard.Shadow = shadow;
        }
        
        // UI polish: Subtle shadow on search field
        if (SearchBorder != null)
        {
            var searchShadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
            SearchBorder.Shadow = searchShadow;
        }
    }
}

