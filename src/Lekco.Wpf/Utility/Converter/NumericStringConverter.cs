using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    public class NumericStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? input = value as string;

            if (string.IsNullOrWhiteSpace(input))
            {
                return Binding.DoNothing;
            }

            try
            {
                return System.Convert.ChangeType(input, targetType, culture);
            }
            catch
            {
                return new ValidationResult("输入无效");
            }
        }
    }
}
