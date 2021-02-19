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
        private Func<IEnumerable<ICommandItem>> subItems = null;   

        public IEnumerable<InputBinding> InputBindings 
        {
            get
            {
                var inputBindings = new List<InputBinding>();           
                
                if (string.IsNullOrEmpty(shortcut) == false)
                {
                    
                    inputBindings.Add(new InputBinding(
                        Command,
                        (KeyGesture)new ExKeyGestureConverter().ConvertFromString(shortcut)                        
                    ));
                }
                
                if(subItems != null)
                {
                    inputBindings.AddRange(subItems().SelectMany(subItem => subItem.InputBindings));
                }

                return inputBindings.AsReadOnly();
            }                
        }

        public string Name
        {
            get;
            private set;
        }

        private string shortcut;
        public string Shortcut
        {
            get => shortcut ?? (subItems?.Invoke().Count() > 0 ? "\U0001f782" : "");
            private set
            {
                shortcut = value;                
            }
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

        public CommandItemViewModel(string name, IEnumerable<ICommandItem> subItems, string shortCut = null) 
            : this(name, new Func<IEnumerable<ICommandItem>>(() => subItems), shortCut)
        {

        }

        public CommandItemViewModel(string name, Func<IEnumerable<ICommandItem>> subItems, string shortCut = null)
        {
            if (subItems == null)
            {
                throw new ArgumentNullException("subItems");
            }

            Name = name;
            Shortcut = shortCut;
            Command = new RelayCommand(() =>
            {
                var content = new CommandPaletteViewModel(subItems.Invoke().Where(i => i.Command.CanExecute(null)));
                Messenger.Default.Send(new OverlayViewModel(content));
            },
            () =>
            {
                return subItems.Invoke().Where(i => i.Command.CanExecute(null)).Count() > 0;
            });

            this.subItems = subItems;
        }
    }
}
