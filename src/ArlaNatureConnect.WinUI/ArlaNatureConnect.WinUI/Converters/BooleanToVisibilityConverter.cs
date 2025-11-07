using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ArlaNatureConnect.WinUI.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value: true => Visible, false => Collapsed.
    /// Supports an optional "invert" parameter (any non-null value) to invert behavior.
    /// </summary>
    public sealed partial class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool visible = false;

            // Check if the input value is a boolean
            if (value is bool b)
            {
                visible = b;
            }

            // If parameter is provided (not null), invert the result
            if (parameter != null)
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility v)
            {
                var result = v == Visibility.Visible;
                if (parameter != null)
                {
                    result = !result;
                }
                return result;
            }

            return false;
        }
    }
}
