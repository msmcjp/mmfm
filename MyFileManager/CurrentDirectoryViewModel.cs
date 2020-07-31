using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyFileManager
{
    public class CurrentDirectoryViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public event EventHandler CurrentChanged;

        private Stack<FileViewModel> selectionStack;
        private FileViewModel[] rootDirectories;
        private bool isFocused;
        private ObservableCollection<FileViewModel> subDirectories;
        private FileViewModel selectedItem;

        public CurrentDirectoryViewModel(KeyValuePair<string, string>[] roots)
        {
            selectionStack = new Stack<FileViewModel>();
            rootDirectories = roots.Select(e => new FileViewModel(e.Key, e.Value)).ToArray();
            SubDirectories = new ObservableCollection<FileViewModel>(rootDirectories);
        }

        private FileViewModel Current => selectionStack.Count == 0 ? new FileViewModel("") : selectionStack.Peek();

        public string Titlet => selectionStack.Count == 0 ? "" : selectionStack.Peek().Name;

        public string FullPath => selectionStack.Count == 0 ? "" : selectionStack.Peek().Path;        

        public ObservableCollection<FileViewModel> SubDirectories 
        { 
            get => subDirectories;
            private set
            {
                subDirectories = value;
                OnPropertyChanged("SubDirectories");
                OnPropertyChanged("SelectedItems");
            }
        }

        public bool IsFocused
        {
            get => isFocused;
            set
            {
                isFocused = value;
                OnPropertyChanged("IsFocused");
            }
        }

        public FileViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public FileViewModel[] SelectedItems => SubDirectories.Where(item => item.IsSelected).ToArray();              

        public ICommand SelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(SelectedItem != null) 
                    {
                        SelectedItem.IsSelected = !SelectedItem.IsSelected;
                    }
                });
            }
        }

        public ICommand LaunchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(SelectedItem == null)
                    {
                        return;
                    }

                    if (SelectedItem.Name == "..")
                    {
                        selectionStack.Pop();
                    }
                    else
                    {
                        selectionStack.Push(SelectedItem);
                    }
                    OnCurrentChanged();
                });
            }
        }

        public ICommand BackCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (selectionStack.Count > 0)
                    {
                        selectionStack.Pop();
                        OnCurrentChanged();
                    }
                });
            }
        }

        private FileViewModel[] ExtractDirectory(string directoryName)
        {
            if(Directory.Exists(directoryName) == false)
            {
                return null;
            }

            var subDirectories = new SortedDictionary<string, string>();
            subDirectories.Add(directoryName, "..");

            try
            {
                foreach (var path in Directory.GetDirectories(directoryName))
                {
                    subDirectories.Add(path, Path.GetFileName(path));
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            return subDirectories.Select(e => e.Value != ".." ? new FileViewModel(e.Key) : new FileViewModel(e.Key , e.Value)).ToArray();
        }

        protected void OnCurrentChanged()
        {
            foreach(var item in SubDirectories)
            {
                item.PropertyChanged -= SubDirectory_PropertyChanged;
            }
            SubDirectories = new ObservableCollection<FileViewModel>(selectionStack.Count == 0 ? rootDirectories : ExtractDirectory(Current.Path));
            foreach (var item in SubDirectories)
            {
                item.PropertyChanged += SubDirectory_PropertyChanged;
            }

            OnPropertyChanged("Title");
            OnPropertyChanged("FullPath");
            CurrentChanged?.Invoke(this, new EventArgs());
        }

        private void SubDirectory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SubDirectories");
        }
    }
}
