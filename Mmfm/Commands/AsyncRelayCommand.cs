using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm.Commands
{
    public class AsyncRelayCommand<T> : AsyncRelayCommandBase
    {
        private Func<T, Task> _actionTask;
        private Func<T, bool> _canExecute;

        public AsyncRelayCommand(Func<T, Task> actionTask)
        {
            _actionTask = actionTask;
        }

        public AsyncRelayCommand(Func<T, Task> actionTask, Func<T, bool> canExecute) : this(actionTask)
        {
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) => IsExecuting == false && (parameter is T) && (_canExecute?.Invoke((T)parameter) ?? true);

        public override async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    IsExecuting = true;
                    await _actionTask((T)parameter);
                }
                finally
                {
                    IsExecuting = false;
                }
            }
        }
    }

    public class AsyncRelayCommand : AsyncRelayCommandBase
    {
        private Func<Task> _actionTask;
        private Func<bool> _canExecute;

        public AsyncRelayCommand(Func<Task> actionTask)
        {
            _actionTask = actionTask;
        }

        public AsyncRelayCommand(Func<Task> actionTask, Func<bool> canExecute) : this(actionTask)
        {
            _canExecute = canExecute;
        }     

        public override bool CanExecute(object parameter = null) => IsExecuting == false && (_canExecute?.Invoke() ?? true);

        public override async void Execute(object parameter)
        {
            if (CanExecute())
            {
                try
                {
                    IsExecuting = true;
                    await _actionTask();
                }
                finally
                {
                    IsExecuting = false;
                }
            }
        }
    }
}
