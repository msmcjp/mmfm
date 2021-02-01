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

        public CurrentDirectoryViewModel CurrentDirectory { get; } = new CurrentDirectoryViewModel();

        public FilesViewModel Files { get; } = new FilesViewModel();

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

        public string[] SelectedPaths
        {
            get
            {
                var targets = CurrentDirectory.SelectedItems.Concat(Files.SelectedItems).Select(i => i.Path).ToArray();
                if (targets.Count() == 0 && SelectedItem != null && SelectedItem.IsNotAlias)
                {
                    targets = new string[1] { SelectedItem.Path };
                }
                return targets;
            }
        }

        private FileViewModel lastSelectedItem = null;
        public FileViewModel SelectedItem => lastSelectedItem;

        public string SelectionStatusText
        {
            get
            {
                var fc = Files.SelectedItems.Count();
                var dc = CurrentDirectory.SelectedItems.Count();

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
            CurrentDirectory.CurrentChanged += CurrentDirectory_CurrentChanged;
            CurrentDirectory.PropertyChanged += CurrentDirectory_PropertyChanged;
            Files.PropertyChanged += Files_PropertyChanged;
            CurrentDirectory.Roots = DefaultFolderShortcuts.PC();
        }

        private void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CurrentDirectory.Roots = DefaultFolderShortcuts.PC().Concat(Favorites).ToArray();
        }

        private void CurrentDirectory_CurrentChanged(object sender, EventArgs e)
        {
            lastSelectedItem = null;

            foreach(var item in Files.Items)
            {
                item.PropertyChanged -= File_PropertyChanged;
            }

            var files = new ObservableCollection<FileViewModel>();
            if (Directory.Exists(CurrentDirectory.FullPath) == true)
            {
                try
                {
                    foreach (var path in Directory.GetFiles(CurrentDirectory.FullPath))
                    {
                        var item = new FileViewModel(path);
                        item.PropertyChanged += File_PropertyChanged;
                        files.Add(item);
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            }

            Files.Items = files;

            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void File_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Files");
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void Files_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem" && Files.SelectedItem != null)
            {
                lastSelectedItem = Files.SelectedItem;
            }
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }

        private void CurrentDirectory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem" && CurrentDirectory.SelectedItem != null)
            {
                lastSelectedItem = CurrentDirectory.SelectedItem;
            }
            OnPropertyChanged("SelectionStatusText");
            OnPropertyChanged("SelectedPaths");
        }
    }
}
