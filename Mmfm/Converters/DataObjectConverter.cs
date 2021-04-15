using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mmfm.Converters
{
    [ValueConversion(typeof(IDataObject), typeof(object))]
    public class DataObjectConverter : DependencyObject, IValueConverter
    {
        #region DataFormat Property
        public static readonly DependencyProperty DataFormatProperty = DependencyProperty.Register(
            "DataFormat",
            typeof(string),
            typeof(DataObjectConverter),
            new FrameworkPropertyMetadata(default(string))
        );

        public string DataFormat
        {
            get => GetValue(DataFormatProperty) as string;
            set => SetValue(DataFormatProperty, value);
        }
        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataObject = value as IDataObject;
            if(dataObject == null || dataObject.GetDataPresent(DataFormat) == false)
            {
                return Binding.DoNothing;
            }
            return dataObject.GetData(DataFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
