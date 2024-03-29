﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(IEnumerable<string>), typeof(string))]
    public class EnumerableJoinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join((string)parameter, (IEnumerable<string>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
