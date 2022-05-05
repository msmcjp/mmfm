using Mmfm.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mmfm.ViewModel
{
    public class DualFileManagerViewModel : INotifyPropertyChanged, IDisposable
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
                var roots = value ?? new FolderShortcutViewModel[] { };
                First.Navigation.Roots = Second.Navigation.Roots = roots;
            }
        }

        //public void Refresh()
        //{
        //    First.Navigation.Refresh();
        //    Second.Navigation.Refresh();
        //}

        private void FileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsActive" && ((FileManagerViewModel)sender).IsActive)
            {
                ActiveFileManager = (FileManagerViewModel)sender;
                OnPropertyChanged(nameof(ActiveFileManager));
            }
        }

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            if(disposed)
            {
                return;
            }

            First.Dispose();
            Second.Dispose();            
        }
        #endregion
    }
}
