using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
{
    public class RelayCommand<T> : ICommand 
    {
        private Action<T> _action;
        private Func<T, bool> _canExecute;

        public RelayCommand(Action<T> action) : this(action, null) { }

        public RelayCommand(Action<T> action, Func<T, bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            bool canExecute =  _action != null && parameter is T;

            if(_canExecute != null && canExecute)
            {
                canExecute &= _canExecute((T)parameter);
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }
    }

    public class RelayCommand : ICommand
    {
        private Action _action;
        private Func<bool> _canExecute;

        public RelayCommand(Action action) : this(action, null) { } 

        public RelayCommand(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute();
            }
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }
}
