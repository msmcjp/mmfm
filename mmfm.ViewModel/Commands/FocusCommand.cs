using Microsoft.UI.Xaml;
using System;
using System.Windows.Input;

namespace Mmfm.ViewModel.Commands
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
            element.Focus(FocusState.Keyboard);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
