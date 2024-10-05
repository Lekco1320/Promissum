using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => new FileSize((long)value).ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
