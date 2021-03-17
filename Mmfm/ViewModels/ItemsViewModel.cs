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
                var selectedIndex = items.IndexOf(lastSelectedItem);
                foreach (var item in Items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                items = value;
                foreach (var item in Items)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }

                if (selectedIndex < 0 || items.Count == 0)
                {
                    SelectedItem = default(T);
                }
                else if (items.Contains(lastSelectedItem))
                {
                    SelectedItem = lastSelectedItem;
                }
                else
                {
                    SelectedItem = items[Math.Min(items.Count - 1, selectedIndex)];
                }

                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(OrderedItems));
            }
        }

        private T selectedItem, lastSelectedItem;       
        public T SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                if(value?.Equals(default(T)) == false)
                {
                    lastSelectedItem = value;
                }
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
        protected ItemsViewModel(IDictionary<string, ISortDescription<T>> sortDescriptions)
        {
            var dict = (IDictionary<string, object>)this.sortDescriptions;
            foreach (var desc in sortDescriptions)
            {
                dict.Add(desc.Key, desc.Value);
            }
        }

        private ExpandoObject sortDescriptions = new ExpandoObject();

        public dynamic SortDescriptions => sortDescriptions;

        public IDictionary<string, ISortDescription<T>> SortDescriptionsDictionary
        {
            get
            {
                return ((IDictionary<string, object>)sortDescriptions).ToDictionary(
                    x => x.Key,
                    x => x.Value as ISortDescription<T>
                );
            }
        }

        private bool canSort = true;
        public bool CanSort
        {
            get => canSort;
            set
            {
                canSort = value;
                OnPropertyChanged(nameof(CanSort));
                OnPropertyChanged(nameof(OrderedItems));
            }
        }

        public ObservableCollection<T> OrderedItems
        {
            get
            {
                if(CanSort && SortDescription != null)
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

        private ISortDescription<T> sortDescription;
        public ISortDescription<T> SortDescription
        {
            get => sortDescription;          
            set
            {
                sortDescription = value;

                // Other description's IsDescending property must be null 
                foreach (var aSortDescription in ((IDictionary<string, object>)sortDescriptions).Values)
                {
                    if (aSortDescription != value)
                    { 
                        (aSortDescription as ISortDescription<T>).IsDescending = null;
                    }
                }

                OnPropertyChanged(nameof(SortDescription));
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
                    sortCommand = new RelayCommand<ISortDescription<T>>((desc) => {
                        desc.ToggleIsDescending();
                        SortDescription = desc;
                    });
                }
                return sortCommand;
            }
        }
        #endregion
    }
}
