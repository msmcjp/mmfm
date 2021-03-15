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

        string DisplayName { get; }

        string Shortcut { get; }

        ICommand Command { get; }

        InputBinding InputBinding { get; }

        IEnumerable<ICommandItem> SubCommands { get; }
    }

    public static class ICommandItemExtension
    {
        public static IEnumerable<ICommandItem> Flatten(this IEnumerable<ICommandItem> commandItems) => commandItems.SelectMany(commandItem => commandItem.Flatten());

        public static IEnumerable<ICommandItem> Flatten(this ICommandItem commandItem) => (new ICommandItem[] { commandItem }).Concat(commandItem.SubCommands?.Flatten());
    }

}
