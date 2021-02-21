using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
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

        private bool ordered;

        public CommandPaletteViewModel(IEnumerable<ICommandItem> allCommandItems, bool ordered)
        {
            AllCommandItems = allCommandItems;
            Items = new List<ICommandItem>(allCommandItems);
            SelectedItem = Items.FirstOrDefault();
            InputText = "";
            this.ordered = ordered;
        }

        private string inputText;
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged(nameof(InputText));
                OnInputTextChanged();
            }
        }

        private ICommandItem selectedItem;
        public ICommandItem SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private IEnumerable<ICommandItem> allCommandItems;
        public IEnumerable<ICommandItem> AllCommandItems
        {
            get => allCommandItems;
            private set
            {
                allCommandItems = value;
                OnPropertyChanged(nameof(AllCommandItems));
            }
        }

        private List<ICommandItem> items;
        public List<ICommandItem> Items
        {
            get => items;
            private set
            {
                items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public bool IsInputTextNullOrEmpty 
        { 
            get => string.IsNullOrEmpty(InputText); 
        }

        private void OnInputTextChanged()
        {
            var items = allCommandItems.Where(c => c.Name.Contains(InputText, StringComparison.CurrentCultureIgnoreCase));
            if (ordered)
            {
                items = items.OrderBy(c => c.Name);
            }
            Items = items.ToList();
                
            SelectedItem = Items.FirstOrDefault();
            OnPropertyChanged(nameof(IsInputTextNullOrEmpty));
        }
    }
}
