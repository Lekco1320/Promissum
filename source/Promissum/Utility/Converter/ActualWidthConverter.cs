using System;
using System.Globalization;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(double), typeof(double))]
    class ActualWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            double x = System.Convert.ToDouble(parameter);
            if (x <= 0d)
            {
                x = 1.0d;
            }
            return d * x;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
