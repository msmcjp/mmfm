using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public class CommandItemViewModel
    {
        public static implicit operator InputBinding(CommandItemViewModel vm) => new InputBinding(vm.Command, (KeyGesture)new KeyGestureConverter().ConvertFromString(vm.Shortcut));

        public string Name
        {
            get;
            private set;
        }

        public string Shortcut
        {
            get;
            private set;
        }

        public ICommand Command
        {
            get;
            private set;
        }

        public CommandItemViewModel(string name, string shortcut, ICommand command)
        {
            Name = name;
            Shortcut = shortcut;
            Command = command;
        }
    }
}
