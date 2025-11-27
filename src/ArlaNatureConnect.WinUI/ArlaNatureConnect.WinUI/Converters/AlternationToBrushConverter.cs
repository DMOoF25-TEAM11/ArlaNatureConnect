using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

using Windows.UI;

namespace ArlaNatureConnect.WinUI.Converters;

public sealed class AlternationToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Expect alternation index (int). If odd -> return alternate background.
        if (value is int idx)
        {
            // Choose two subtle background colors; adjust as desired.
            SolidColorBrush odd = new SolidColorBrush(Color.FromArgb(0xFF, 0x2B, 0x2B, 0x2B)); // darker
            SolidColorBrush even = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20)); // slightly lighter
            return (idx % 2) == 1 ? odd : even;
        }

        // Fallback: transparent
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
