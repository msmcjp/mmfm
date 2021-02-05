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
    public class NavigationViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public event EventHandler CurrentChanged;

        private Stack<FileViewModel> navigationStack = new Stack<FileViewModel>();
        private FolderShortcutViewModel[] roots;

        public NavigationViewModel()
        {
            Folders = new FoldersViewModel((item) => {
                if (item.Name == "..")
                {
                    BackToParent();
                }
                else
                {
                    Current = item;
                }
            });
            Folders.PropertyChanged += Items_PropertyChanged;

            Files = new FilesViewModel();
            Files.PropertyChanged += Items_PropertyChanged;
        }

        public FileViewModel Current
        {
            get => navigationStack.Count == 0 ? null : navigationStack.Peek();
            set
            {
                if (value != null)
                {
                    navigationStack.Push(value);
                    OnCurrentChanged();
                }
                else
                {
                    navigationStack.Clear();
                    OnCurrentChanged();
                }
            }
        }

        private FileViewModel selectedItem;
        public FileViewModel SelectedItem
        {
            get => selectedItem;
            private set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public bool IsRoot => navigationStack.Count == 0;

        public bool IsNotRoot => !IsRoot;

        public string Title => FullPath.Replace("\\", " \U0001f782 ");

        public string FullPath => navigationStack.Count == 0 ? "" : navigationStack.Peek().Path;

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

        public FoldersViewModel Folders { get; private set; }

        public FilesViewModel Files { get; private set; }

        public IList<FileViewModel> SelectedItems
        {
            get
            {
                var selectedItems = Folders.SelectedItems.Concat(Files.SelectedItems);
                if(selectedItems.Count() == 0)
                {
                    selectedItems = new FileViewModel[] { SelectedItem };
                }
                return Array.AsReadOnly(selectedItems.ToArray());
            }
        }

        public IList<FileViewModel> Items => Array.AsReadOnly(Folders.Items.Concat(Files.Items).ToArray());    

        public void BackToParent()
        {
            if (navigationStack.Count > 0)
            {
                OnCurrentChanged(navigationStack.Pop());
            }
        }

        public void Refresh()
        {
            Folders.Items = new ObservableCollection<FileViewModel>(navigationStack.Count == 0 ? roots.Select(r => (FileViewModel)r) : ExtractDirectory(Current.Path));
            if (Current != null)
            {
                Files.Items = new ObservableCollection<FileViewModel>(Directory.GetFiles(Current.Path).Select(p => new FileViewModel(p)));
            }
            else
            {
                Files.Items = new ObservableCollection<FileViewModel>();
            }
        }

        private FileViewModel[] ExtractDirectory(string directoryName)
        {
            if(Directory.Exists(directoryName) == false)
            {
                return null;
            }

            var items = new List<FileViewModel>();
            items.Add(FileViewModel.CreateAlias(directoryName, ".."));
            try
            {
                foreach (var path in Directory.GetDirectories(directoryName))
                {
                    items.Add(new FileViewModel(path));
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            return items.ToArray();
        }

        protected void OnCurrentChanged(FileViewModel selectedItem = null)
        {
            Refresh();

            if(selectedItem != null && Folders.Items.Contains(selectedItem)) 
            {
                Folders.SelectedItem = selectedItem;
            }
            else
            {
                Folders.SelectedItem = Folders.Items.First();
            }
   
            OnPropertyChanged("Title");
            OnPropertyChanged("FullPath");
            OnPropertyChanged("IsRoot");
            OnPropertyChanged("IsNotRoot");
            
            CurrentChanged?.Invoke(this, new EventArgs());
        }

        private void Items_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                SelectedItem = (sender as ItemsViewModel<FileViewModel>).SelectedItem;
            }

            if(e.PropertyName == "SelectedItems")
            {
                OnPropertyChanged("SelectedItems");
            }
        }
    }
}
