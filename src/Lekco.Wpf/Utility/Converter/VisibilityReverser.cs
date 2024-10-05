using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class VisibilityReverser : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
}
