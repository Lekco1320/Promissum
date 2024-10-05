using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Lekco.Promissum.Utility.Converter
{
    [ValueConversion(typeof(string), typeof(Bitmap))]
    public class FileIconGetter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                var extension = Path.GetExtension(path);
                bool largeIcon = (parameter as bool?) ?? false;
                return SHFileInfoHelper.GetFileIconImage(extension, largeIcon);
            }
            throw new ArgumentException("Invalid value.", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
