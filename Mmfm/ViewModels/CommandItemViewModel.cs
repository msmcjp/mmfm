using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace Mmfm
{
    public class CommandItemViewModel : ICommandItem
    {
        public static readonly string SeparatorString = "/";

        public static readonly string DisplaySeparatorString = " \U0001f782 ";

        private Func<ICommandItem, IEnumerable<ICommandItem>> subCommands = null;

        public InputBinding InputBinding => string.IsNullOrEmpty(shortcut) ? 
            null : new InputBinding(Command, (KeyGesture)new ExKeyGestureConverter().ConvertFromString(shortcut));

        public IEnumerable<ICommandItem> SubCommands => subCommands?.Invoke(this) ?? Enumerable.Empty<ICommandItem>();
       
        public string Name
        {
            get;
            private set;
        }     

        private string shortcut;
        public string Shortcut
        {
            get => shortcut ?? (subCommands?.Invoke(this).Count() > 0 ? "\U0001f782" : "");
            private set
            {
                shortcut = value;                
            }
        }

        public string DisplayName
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
            DisplayName = name.Split(SeparatorString).Last();
            Shortcut = shortcut;
            Command = command;
        }

        public CommandItemViewModel(string name, IEnumerable<ICommandItem> subItems, bool ordered = false, string shortCut = null)
            : this(name, new Func<ICommandItem, IEnumerable<ICommandItem>>((parent) => subItems), ordered, shortCut)
        {
        }

        public CommandItemViewModel(string name, Func<ICommandItem, IEnumerable<ICommandItem>> subCommands, bool ordered = false, string shortCut = null)
        {
            if (subCommands == null)
            {
                throw new ArgumentNullException();
            }

            Name = name;
            DisplayName = name.Split(SeparatorString).Last();
            Shortcut = shortCut;
            Command = new RelayCommand(() =>
            {
                var content = new CommandPaletteViewModel(subCommands.Invoke(this).Where(i => i.Command.CanExecute(null)), ordered);
                Messenger.Default.Send(new OverlayViewModel(content));
            },
            () =>
            {
                return subCommands.Invoke(this).Where(i => i.Command.CanExecute(null)).Count() > 0;
            });

            this.subCommands = subCommands;
        }

        public CommandItemViewModel(ICommandItem baseItem)
        {
            Name = baseItem.Name;
            DisplayName = baseItem.Name.Replace(SeparatorString, DisplaySeparatorString);
            Shortcut = baseItem.Shortcut;
            Command = baseItem.Command;
        }
    }
}
