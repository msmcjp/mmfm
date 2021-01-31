using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mmfm
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

        private Stack<FileViewModel> selectionStack = new Stack<FileViewModel>();
        private FolderShortcutViewModel[] roots;
        private ObservableCollection<FileViewModel> subDirectories = new ObservableCollection<FileViewModel>();
        private FileViewModel selectedItem;

        public CurrentDirectoryViewModel()
        {
        }

        public FileViewModel Current
        {
            get => selectionStack.Count == 0 ? new FileViewModel("", "") : selectionStack.Peek();
            set
            {
                if(value != null)
                {
                    selectionStack.Push(value);
                    OnCurrentChanged(null);
                }
                else
                {
                    selectionStack.Clear();
                    OnCurrentChanged(null);
                }
            }
        }

        public bool IsRoot => selectionStack.Count == 0;

        public bool IsNotRoot => !IsRoot;

        public string Title => FullPath.Replace("\\", " \U0001f782 ");

        public string FullPath => selectionStack.Count == 0 ? "" : selectionStack.Peek().Path;

        public FolderShortcutViewModel[] Roots
        {
            get => roots;
            set
            {
                roots = value;
                OnPropertyChanged("Roots");
                if (IsRoot)
                {
                    OnCurrentChanged();
                }
            }
        }

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

        public FileViewModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public FileViewModel[] SelectedItems
        {
            get => SubDirectories.Where(item => item.IsSelected && item.IsNotAlias).ToArray();
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

        public ICommand LaunchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedItem == null)
                    {
                        return;
                    }

                    if (SelectedItem.Name == "..")
                    {
                        OnCurrentChanged(selectionStack.Pop());
                    }
                    else
                    {
                        selectionStack.Push(SelectedItem);
                        OnCurrentChanged();
                    }
                });
            }
        }

        public void BackToParent()
        {
            if (selectionStack.Count > 0)
            {
                OnCurrentChanged(selectionStack.Pop());
            }
        }

        public void RaiseCurrentChanged()
        {
            OnCurrentChanged();
        }

        private FileViewModel[] ExtractDirectory(string directoryName)
        {
            if(Directory.Exists(directoryName) == false)
            {
                return null;
            }

            var subDirectories = new List<FileViewModel>();
            
            subDirectories.Add(FileViewModel.CreateAlias(directoryName, ".."));
            try
            {
                foreach (var path in Directory.GetDirectories(directoryName))
                {
                    subDirectories.Add(new FileViewModel(path));
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            return subDirectories.ToArray();
        }

        protected void OnCurrentChanged(FileViewModel selectedItem = null)
        {
            foreach(var item in SubDirectories)
            {
                item.PropertyChanged -= SubDirectory_PropertyChanged;
            }

            SubDirectories = new ObservableCollection<FileViewModel>(selectionStack.Count == 0 ? roots.Select(r => (FileViewModel)r) : ExtractDirectory(Current.Path));

            foreach (var item in SubDirectories)
            {
                item.PropertyChanged += SubDirectory_PropertyChanged;
            }

            if(selectedItem != null)
            {
                SelectedItem = selectedItem;
            }

            OnPropertyChanged("Title");
            OnPropertyChanged("FullPath");
            OnPropertyChanged("IsRoot");
            OnPropertyChanged("IsNotRoot");
            CurrentChanged?.Invoke(this, new EventArgs());
        }

        private void SubDirectory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("SubDirectories");
        }
    }
}
