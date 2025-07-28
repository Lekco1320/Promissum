using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Wpf.Utility.Converter
{
    [ValueConversion(typeof(Enum), typeof(int))]
    public class EnumIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            return System.Convert.ToInt32(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return Binding.DoNothing;
            }

            try
            {
                var enumType = parameter as Type;
                if (enumType == null || !enumType.IsEnum)
                {
                    return Binding.DoNothing;
                }

                return Enum.ToObject(enumType, value);
            }
            catch
            {
                return Binding.DoNothing;
            }
        }
    }
}
