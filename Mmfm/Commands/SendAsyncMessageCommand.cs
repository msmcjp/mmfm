using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm.Commands
{
    public class SendAsyncMessageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Type MessageType
        {
            get;
            set;
        }

        private object ConvertParameter(object parameter)
        {
            if(parameter == null || MessageType == null) 
            { 
                return null; 
            }

            if (MessageType.IsAssignableFrom(parameter.GetType()) == true) 
            { 
                return parameter; 
            }       

            var attr = Attribute.GetCustomAttribute(MessageType, typeof(TypeConverterAttribute)) as TypeConverterAttribute;
            var converter = Activator.CreateInstance(Type.GetType(attr?.ConverterTypeName)) as TypeConverter;
            
            if (converter?.CanConvertFrom(parameter.GetType()) == true)
            {
                return converter.ConvertFrom(parameter);
            }

            return null;
        }

        public bool CanExecute(object parameter) => ConvertParameter(parameter) != null;

        public async void Execute(object parameter) => await Task.Run(() => typeof(Messenger)
            .GetMethod("SendAsync")
            .MakeGenericMethod(MessageType)
            .Invoke(Messenger.Default, new object[] { ConvertParameter(parameter) })
        );
    }
}
