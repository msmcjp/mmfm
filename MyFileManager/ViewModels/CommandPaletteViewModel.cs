using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
{
    public class CommandPaletteViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private readonly CommandItemViewModel[] commandItems;

        public CommandPaletteViewModel(CommandItemViewModel[] commandItems)
        {
            this.commandItems = commandItems;
            Items = new List<CommandItemViewModel>(commandItems);
            SelectedItem = Items.FirstOrDefault();
            InputText = "";
        }

        private string inputText;
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged("InputText");
                OnInputTextChanged();
            }
        }

        private CommandItemViewModel selectedItem;
        public CommandItemViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        private List<CommandItemViewModel> items;
        public List<CommandItemViewModel> Items
        {
            get => items;
            private set
            {
                items = value;
                OnPropertyChanged("Items");
            }
        }

        public bool IsInputTextNullOrEmpty 
        { 
            get => string.IsNullOrEmpty(InputText); 
        }

        private void OnInputTextChanged()
        {
            Items = new List<CommandItemViewModel>(commandItems.Where(c => c.Name.Contains(InputText, StringComparison.CurrentCultureIgnoreCase)).OrderBy(c => c.Name));
            SelectedItem = Items.FirstOrDefault();
            OnPropertyChanged("IsInputTextNullOrEmpty");
        }
    }
}
