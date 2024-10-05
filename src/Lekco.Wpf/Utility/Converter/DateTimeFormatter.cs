using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateTimeFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (parameter is string s)
                {
                    return dateTime.ToString(s);
                }
                return dateTime.ToString();
            }
            throw new ArgumentException("Invalid date time type.", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
