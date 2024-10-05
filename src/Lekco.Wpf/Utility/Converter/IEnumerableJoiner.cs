using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(IEnumerable<string>), typeof(string))]
    public class IEnumerableJoiner : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<string> source;
            if (value is IEnumerable<string>)
            {
                source = (IEnumerable<string>)value;
            }
            else if (value is IEnumerable enumerable)
            {
                var list = new List<string>();
                foreach (object item in enumerable)
                {
                    list.Add(item.ToString() ?? "");
                }
                source = list;
            }
            else
            {
                throw new ArgumentException("Given value is not enumerable.", nameof(value));
            }

            
            if (parameter is string separator)
            {
                return string.Join(separator, source);
            }
            else
            {
                return string.Join(" ", source);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
