using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(Enum), typeof(int))]
    public class EnumIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
