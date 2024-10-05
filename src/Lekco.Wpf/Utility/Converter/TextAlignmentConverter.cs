using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(HorizontalAlignment), typeof(TextAlignment))]
    public class TextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                HorizontalAlignment.Left => TextAlignment.Left,
                HorizontalAlignment.Right => TextAlignment.Right,
                HorizontalAlignment.Center => TextAlignment.Center,
                HorizontalAlignment.Stretch => TextAlignment.Justify,
                _ => throw new ArgumentException("Invalid value.", nameof(value))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                TextAlignment.Left => HorizontalAlignment.Left,
                TextAlignment.Right => HorizontalAlignment.Right,
                TextAlignment.Center => HorizontalAlignment.Center,
                TextAlignment.Justify => HorizontalAlignment.Stretch,
                _ => throw new ArgumentException("Invalid value.", nameof(value))
            };
        }
    }
}
