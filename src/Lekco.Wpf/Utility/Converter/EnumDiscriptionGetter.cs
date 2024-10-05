using System;
using System.Globalization;
using System.Windows.Data;
using Lekco.Wpf.Utility.Helper;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumDiscriptionGetter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum @enum)
            {
                return @enum.GetDiscription();
            }
            throw new ArgumentException("Invalid enum value.", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
