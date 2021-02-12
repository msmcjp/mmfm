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
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(OrderedItems));
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
                OnPropertyChanged(nameof(SelectedItem));
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

        private IEnumerable<ISortDescription<T>> orderBy;
        public IEnumerable<ISortDescription<T>> OrderBy
        {
            get => orderBy;
            set
            {
                orderBy = value;
                OnPropertyChanged(nameof(SortDescription));
                OnPropertyChanged(nameof(OrderedItems));
            }
        }
       
        public ObservableCollection<T> OrderedItems
        {
            get 
            {
                if (OrderBy?.Count() > 0)
                {
                    var orderedItems = OrderBy.Aggregate(
                        (IOrderedQueryable<T>)Items.AsQueryable<T>(),
                        (queryable, desc) => OrderBy.First() == desc ? desc.OrderBy(queryable) : desc.ThenBy(queryable)
                    );
                    return new ObservableCollection<T>(orderedItems);
                }
                return Items;
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

        public abstract ICommand SortCommand { get; }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SelectedItem.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedItems));
            }
        }
    }
}
