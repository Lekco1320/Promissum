using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(long), typeof(string))]
    public class DiskLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Functions.FormatBytesLength((long)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

    }
}
