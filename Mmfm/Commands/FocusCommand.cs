using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mmfm.Commands
{
    public class FocusCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter is FrameworkElement;
        }

        public void Execute(object parameter)
        {
            var element = (parameter as FrameworkElement);
            element.Dispatcher.InvokeAsync(() => element.Focus());
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
