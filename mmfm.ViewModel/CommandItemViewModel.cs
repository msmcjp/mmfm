using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Mmfm.ViewModel
{
    public class CommandItemViewModel : ICommandItem
    {
        public static readonly string PathSeparator = "\U0001f782";

        private Func<ICommandItem, IEnumerable<ICommandItem>> subCommands = null;

        public string KeyGesture
        {
            get;
            set;
        }

        public IEnumerable<ICommandItem> SubCommands => subCommands?.Invoke(this) ?? Enumerable.Empty<ICommandItem>();
       
        public string Name
        {
            get;
            private set;
        }

        public string Shortcut
        {
            get
            {
                //if(KeyGesture != null)
                //{
                //    return new KeyGestureConverter().ConvertToString(KeyGesture);
                //}
                return (subCommands?.Invoke(this).Count() > 0 ? PathSeparator : "");
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
            DisplayName = name.Split(CommandItem.PathSeparator).Last();
            Command = command;
            if(shortcut != null)
            {
                //KeyGesture = (KeyGesture)new KeyGestureConverter().ConvertFromString(shortcut);
            }
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
            DisplayName = name.Split(CommandItem.PathSeparator).Last();
            Command = new Commands.RelayCommand(() =>
            {
                var content = new CommandPaletteViewModel(subCommands.Invoke(this).Where(i => i.Command.CanExecute(null)), ordered);
                Messenger.Default.Send(new OverlayViewModel(content));
            },
            () =>
            {
                return subCommands.Invoke(this).Where(i => i.Command.CanExecute(null)).Count() > 0;
            });

            if(shortCut != null) 
            {
                //KeyGesture = (KeyGesture)new KeyGestureConverter().ConvertFromString(shortCut);
            }

            this.subCommands = subCommands;
        }

        public CommandItemViewModel(ICommandItem baseItem)
        {
            Name = baseItem.Name;
            DisplayName = baseItem.Name.Replace(CommandItem.PathSeparator.ToString(), $" {PathSeparator} ");
            KeyGesture = baseItem.KeyGesture;
            Command = baseItem.Command;
        }
    }
}
