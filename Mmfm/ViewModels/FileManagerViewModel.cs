using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Specialized;

namespace Mmfm
{
    public class FileManagerViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private bool isActive;

        public NavigationViewModel Navigation { get; } = new NavigationViewModel();

        private ObservableCollection<FolderShortcutViewModel> favorites;
        public ObservableCollection<FolderShortcutViewModel> Favorites
        {
            get => favorites;
            set
            {
                if (favorites != null)
                {
                    favorites.CollectionChanged -= Favorites_CollectionChanged;
                }

                favorites = value;

                if (favorites != null)
                {
                    favorites.CollectionChanged += Favorites_CollectionChanged;
                    Favorites_CollectionChanged(
                        this, 
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                    );
                }

                OnPropertyChanged("Favorites");
            }
        }

        public bool IsActive 
        { 
            get => isActive; 
            set
            {
                isActive = value; 
                OnPropertyChanged("IsActive"); 
            } 
        }

        public IList<string> SelectedPaths => Array.AsReadOnly(SelectedItems.Select(item => item.Path).ToArray());

        public IList<FileViewModel> SelectedItems => Navigation.SelectedItems;

        public FileViewModel SelectedItem => Navigation.SelectedItem;

        public string SelectionStatusText
        {
            get
            {
                var fc = Navigation.Files.SelectedItems.Count();
                var dc = Navigation.Folders.SelectedItems.Count();

                if (fc + dc == 0 && SelectedItem != null)
                {
                    _ = SelectedItem.IsFolder ? dc++ : fc++;
                }

                if (fc + dc == 0) { return ""; }

                var text = "";
                if (fc > 0)
                {
                    text += $@"{fc} file{(fc > 1 ? "s" : "")}{(dc > 0 ? " and " : "")}";
                }

                if (dc > 0)
                {
                    text += $@"{dc} folder{(dc > 1 ? "s" : "")}";
                }

                return $"{text} {(fc + dc > 1 ? "are" : "is")} selected.";
            }
        }
      
        public FileManagerViewModel()
        {
            Navigation.Roots = DefaultFolderShortcuts.PC();
            Navigation.PropertyChanged += Navigation_PropertyChanged;
        }

        private void Navigation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectedItems")
            {
                OnPropertyChanged("SelectedItems");
                OnPropertyChanged("SelectedPaths");
                OnPropertyChanged("SelectionStatusText");
            }

            if (e.PropertyName == "SelectedItem")
            {
                OnPropertyChanged("SelectedItem");
                OnPropertyChanged("SelectionStatusText");
            }
        }

        private void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Navigation.Roots = DefaultFolderShortcuts.PC().Concat(Favorites).ToArray();
        }
    }
}
