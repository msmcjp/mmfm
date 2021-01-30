using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public interface ICommandItem
    {
        string Name { get; }

        string Shortcut { get; }

        ICommand Command { get;  }

        IEnumerable<InputBinding> InputBindings { get; }
    }
}
