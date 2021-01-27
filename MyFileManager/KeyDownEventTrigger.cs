using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyFileManager
{
    public class KeyDownEventTrigger : Microsoft.Xaml.Behaviors.EventTrigger
    {
        public static readonly DependencyProperty KeyProperty =
         DependencyProperty.Register(
             "Key",
             typeof(string),
             typeof(KeyDownEventTrigger),
             null
         );

        public KeyDownEventTrigger() : base("KeyDown")
        {
        }

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        protected override void OnEvent(EventArgs eventArgs)
        {
            var e = eventArgs as KeyEventArgs;
            if (e != null && e.Key == (Key)new KeyConverter().ConvertFromString(Key))
            {
                this.InvokeActions(eventArgs);
            }
        }
    }
}
