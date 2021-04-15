using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    public class DropFileListMessageTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(IDataObject).IsAssignableFrom(sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new DropFileListMessage(value as IDataObject);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(DropFileListMessageTypeConverter))]
    public class DropFileListMessage
    {
        public DropFileListMessage(IDataObject dataObject)
        {
            DataObject = dataObject;
        }

        public IDataObject DataObject
        {
            get;
            private set;
        }
    }
}
