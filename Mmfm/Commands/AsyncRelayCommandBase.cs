using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm.Commands
{
    public abstract class AsyncRelayCommandBase : RelayCommandBase
    {
        private bool isExecuting;

        protected bool IsExecuting
        {
            get => isExecuting;
            set
            {
                isExecuting = value;
                RaiseCanExecuteChanged();
            }
        }
    }
}
