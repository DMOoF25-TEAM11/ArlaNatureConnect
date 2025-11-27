using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ArlaNatureConnect.WinUI.Converters
{
    public sealed class BooleanToStyleConverter : IValueConverter
    {
        public Style? TrueStyle { get; set; }
        public Style? FalseStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                bool b = false;
                if (value is bool vb) b = vb;
                return b ? (object?)TrueStyle ?? DependencyProperty.UnsetValue : (object?)FalseStyle ?? DependencyProperty.UnsetValue;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
