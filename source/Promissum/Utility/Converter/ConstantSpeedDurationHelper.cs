using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(double), typeof(Duration))]
    public class ConstantSpeedDurationHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Duration(new TimeSpan(0, 0, 0, 0, (int)((double)value / (double)parameter * 1000)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
