using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NetShare.Converters
{
    public class DragEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DragEventArgs? args = value as DragEventArgs;
            string[]? files = args?.Data?.GetData(DataFormats.FileDrop) as string[];
            return files ?? Array.Empty<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This conversion is not supported!");
        }
    }
}
