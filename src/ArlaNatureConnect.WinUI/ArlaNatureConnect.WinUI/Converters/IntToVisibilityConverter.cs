using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ArlaNatureConnect.WinUI.Converters;

/// <summary>
/// Converts an integer value to a Visibility value: > 0 => Visible, 0 => Collapsed.
/// </summary>
public sealed partial class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int intValue = 0;
        
        if (value is int i)
        {
            intValue = i;
        }
        else if (value is long l)
        {
            intValue = (int)l;
        }

        return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// since this is a one-way converter, we do not implement ConvertBack.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // Not needed for one-way binding
        throw new NotImplementedException();
    }
}

