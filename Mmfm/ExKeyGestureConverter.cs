using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public class ExKeyGestureConverter : TypeConverter
    {
        private KeyGestureConverter converter = new KeyGestureConverter();

        private static IDictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { ",", "OemComma" },
            { ".", "OemPeriod" },
            { ";", "OemSemicolon" },
            { ":", "OemColon" },
            { "/", "Devide" },
            { "*", "Multiply" },
            { "-", "Subtract" },
            { "plus", "Add" },            
            { "0", "D0" },
            { "1", "D1" },
            { "2", "D2" },
            { "3", "D3" },
            { "4", "D4" },
            { "5", "D5" },
            { "6", "D6" },
            { "7", "D7" },
            { "8", "D8" },
            { "9", "D9" },
        };

        private static IDictionary<string, string> dictionary_r = dictionary.ToDictionary(x => x.Value, x => x.Key);

        private Regex regex = new Regex(@"[^\+]+");

        public ExKeyGestureConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
        {
            if(source is string == false)
            {
                return null;
            }

            var keyGesture = dictionary.Keys.Aggregate((string)source, 
                (source, key) => regex.Replace(source, (m) => dictionary.ContainsKey(m.Value) ? dictionary[m.Value] : m.Value));
            return converter.ConvertFromString(keyGesture);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType != typeof(string))
            {
                return null;
            }

            var keyGesture = converter.ConvertToString((string)value);
            return dictionary_r.Keys.Aggregate(keyGesture, 
                (source, key) => regex.Replace(source, (m) => dictionary_r.ContainsKey(m.Value) ? dictionary_r[m.Value] : m.Value));
        }
    }
}
