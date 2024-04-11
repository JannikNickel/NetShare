using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NetShare.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bVal = value as bool?;
            return bVal.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility? vValue = value as Visibility?;
            return vValue == Visibility.Visible;
        }
    }
}

