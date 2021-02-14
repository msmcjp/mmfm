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
                    Goto(item.Path);
                }
            });
            Folders.PropertyChanged += Items_PropertyChanged;

            Files = new FilesViewModel();
            Files.PropertyChanged += Items_PropertyChanged;
        }

        public FileViewModel Current => navigationStack.Count == 0 ? null : navigationStack.Peek();
 
        private FileViewModel selectedItem;
        public FileViewModel SelectedItem
        {
            get => selectedItem;
            private set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
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
                OnPropertyChanged(nameof(Roots));
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
                if (selectedItems.Count() == 0 && SelectedItem?.IsNotAlias == true)
                {
                    selectedItems = new FileViewModel[] { SelectedItem };
                }
                return Array.AsReadOnly(selectedItems.ToArray());
            }
        }

        public IList<FileViewModel> Items => Array.AsReadOnly(Folders.Items.Concat(Files.Items).ToArray());

        public bool Goto(string path)
        {            
            if (Directory.Exists(path) == false)
            {
                return false;
            }

            navigationStack.Push(new FileViewModel(path));
            OnCurrentChanged();

            return true;
        }

        public void GotoTop()
        {
            navigationStack.Clear();
            OnCurrentChanged();
        }

        public bool BackToParent()
        {
            if (navigationStack.Count == 0)
            {
                return false;
            }
            OnCurrentChanged(navigationStack.Pop());
            return true;
        }

        private bool showHiddenFiles;
        public bool ShowHiddenFiles
        {
            get => showHiddenFiles;
            set
            {
                showHiddenFiles = value;
                OnPropertyChanged(nameof(ShowHiddenFiles));
                Refresh();
            }
        }

        private Func<FileViewModel, bool> Predicate => (x => ShowHiddenFiles || !x.IsHidden);

        public void Refresh()
        {
            if (IsRoot)
            {
                Folders.Items = new ObservableCollection<FileViewModel>(roots.Select(r => (FileViewModel)r));
                Files.Items = new ObservableCollection<FileViewModel>();
            }
            else
            {
                var folders = ExtractDirectory(Current.Path).Where(Predicate);
                Folders.Items = new ObservableCollection<FileViewModel>(folders);

                var files = Directory.GetFiles(Current.Path)
                    .Select(p => new FileViewModel(p))
                    .Where(Predicate);
                Files.Items = new ObservableCollection<FileViewModel>(files);
            }
            Folders.CanSort = IsNotRoot;
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
   
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(FullPath));
            OnPropertyChanged(nameof(IsRoot));
            OnPropertyChanged(nameof(IsNotRoot));
            OnPropertyChanged(nameof(Current));
        }

        private void Items_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem))
            {
                SelectedItem = (sender as ItemsViewModel<FileViewModel>).SelectedItem;
                OnPropertyChanged(nameof(SelectedItems));
            }

            if (e.PropertyName == nameof(SelectedItems))
            {
                OnPropertyChanged(nameof(SelectedItems));
            }
        }
    }
}
