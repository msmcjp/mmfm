using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyFileManager
{
    public class HideWindowAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            Window.GetWindow(AssociatedObject).Hide();
        }
    }
}
