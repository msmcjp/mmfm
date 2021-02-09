using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mmfm
{
    public class DualFileManagerViewModel : INotifyPropertyChanged
    { 
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion       

        public FileManagerViewModel First { get; } = new FileManagerViewModel();
        public FileManagerViewModel Second { get; } = new FileManagerViewModel();

        public FileManagerViewModel ActiveFileManager
        {
            get;
            private set;
        }

        public DualFileManagerViewModel()
        {
            First.PropertyChanged += FileManager_PropertyChanged;
            Second.PropertyChanged += FileManager_PropertyChanged;

            Messenger.Default.Register<ClipboardManager.Notification>(this, (n) =>
            {                
                foreach (var item in First.Navigation.Items.Concat(Second.Navigation.Items))
                {
                    item.IsCut = n.Move && n.Paths.Contains(item.Path);
                }
            });
        }

        public IEnumerable<Settings.FileManager> Settings
        {
            get => new List<Settings.FileManager> { First.Settings, Second.Settings }.AsReadOnly();
            set 
            { 
                First.Settings = value?.ElementAt(0); 
                Second.Settings = value?.ElementAt(1); 
            }
        }

        public FolderShortcutViewModel[] Roots
        {
            set
            {
                First.Navigation.Roots = value ?? DefaultFolderShortcuts.PC();
                Second.Navigation.Roots = value ?? DefaultFolderShortcuts.PC();
            }
        }

        public void Refresh()
        {
            First.Navigation.Refresh();
            Second.Navigation.Refresh();
        }

        private void FileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsActive" && ((FileManagerViewModel)sender).IsActive)
            {
                ActiveFileManager = (FileManagerViewModel)sender;
                OnPropertyChanged("ActiveFileManager");
            }
        }
    }
}
