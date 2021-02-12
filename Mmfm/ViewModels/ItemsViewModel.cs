using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
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

        public ICommand ToggleIsSelectedCommand
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

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem.IsSelected))
            {
                OnPropertyChanged(nameof(SelectedItems));
            }
        }

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

        #region "Sorting"
        private IDictionary<string, ISortDescription<T>> sortDescriptions;
        public dynamic SortDescriptions { get; } = new ExpandoObject();

        protected ItemsViewModel(IDictionary<string, ISortDescription<T>> sortDescriptions)
        {
            this.sortDescriptions = sortDescriptions;
            var dict = (IDictionary<string, object>)SortDescriptions;
            foreach (var item in sortDescriptions)
            {
                dict[item.Key] = item.Value;
            }
        }

        public ObservableCollection<T> OrderedItems
        {
            get
            {
                if(SortDescription != null)
                {
                    var orderBy = OrderBy(SortDescription);
                    if (orderBy?.Count() > 0)
                    {
                        var items = orderBy.Aggregate(
                            (IOrderedQueryable<T>)Items.AsQueryable<T>(),
                            (source, desc) => orderBy.First() == desc ? desc.OrderBy(source) : desc.ThenBy(source)
                        );
                        return new ObservableCollection<T>(items);
                    }
                }                
                return Items;
            }
        }

        private ISortDescription<T> SortDescription
        {
            get
            {
                foreach (var sortDescription in sortDescriptions.Values)
                {
                    if (sortDescription.IsDescending != null)
                    {
                        return sortDescription;
                    }
                }
                return null;
            }
            set
            {
                foreach (var sortDescription in sortDescriptions.Values)
                {
                    if (sortDescription == value)
                    {
                        sortDescription.IsDescending = !sortDescription?.IsDescending ?? true;
                    }
                    else
                    {
                        sortDescription.IsDescending = null;
                    }
                }
                OnPropertyChanged(nameof(OrderedItems));
            }
        }

        protected virtual IEnumerable<ISortDescription<T>> OrderBy(ISortDescription<T> current)
        {
            return new ISortDescription<T>[] { current };
        }

        private ICommand sortCommand;
        public ICommand SortCommand
        {
            get
            {
                if (sortCommand == null)
                {
                    sortCommand = new RelayCommand<ISortDescription<T>>((desc) => SortDescription = desc);
                }
                return sortCommand;
            }
        }
        #endregion
    }
}
