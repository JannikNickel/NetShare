using System;
using System.Globalization;
using System.Windows.Data;

namespace NetShare.Converters
{
    public class DoubleToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? dVal = value as double?;
            return dVal * 100.0 ?? 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This conversion is not supported!");
        }
    }
}

