using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class IEnumerableCountGetter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                ICollection collection => collection.Count,
                IEnumerable enumerable => GetCount(enumerable),
                _ => throw new ArgumentException("Given value is not enumerable.", nameof(value)),
            };
        }

        protected static int GetCount(IEnumerable enumerable)
        {
            int count = 0;
            foreach (var item in enumerable)
            {
                ++count;
            }
            return count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
