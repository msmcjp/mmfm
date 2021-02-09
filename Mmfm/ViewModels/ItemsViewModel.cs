using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
{
    public abstract class ItemsViewModel<T> : INotifyPropertyChanged where T : IHasIsSelected, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<T> items = new ObservableCollection<T>();
        public ObservableCollection<T> Items
        {
            get => items;
            set
            {
                var selectedItem = SelectedItem;
                foreach (var item in Items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                items = value;
                foreach (var item in Items)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
                OnPropertyChanged("Items");
                SelectedItem = selectedItem;
            }
        }

        private T selectedItem;
        public T SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public T[] SelectedItems
        {
            get => Items.Where(item => item.IsSelected).ToArray();
        }

        public ICommand SelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItem != null)
                    {
                        SelectedItem.IsSelected = !SelectedItem.IsSelected;
                    }
                });
            }
        }

        protected abstract void Launch(T item);

        public ICommand LaunchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItem != null)
                    {
                        Launch(SelectedItem);
                    }
                });
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsSelected")
            {
                OnPropertyChanged("SelectedItems");
            }
        }
    }
}
