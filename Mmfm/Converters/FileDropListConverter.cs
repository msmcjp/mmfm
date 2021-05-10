using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mmfm.Converters
{
    [ValueConversion(typeof(string[]), typeof(string))]
    public class FileDropListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var paths = value as string[];
            if(paths == null)
            {
                return null;
            }

            var others = paths.Length > 1 ? string.Format(Properties.Resources.FileDropList_Message, paths.Length-1) : "";
            return Path.GetFileName(paths.First()) + others;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
