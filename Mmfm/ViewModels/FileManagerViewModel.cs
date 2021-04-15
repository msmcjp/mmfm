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
    public class FileManagerViewModel : INotifyPropertyChanged, IDisposable
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

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public IList<string> SelectedPaths => Array.AsReadOnly(SelectedItems.Select(item => item.Path).ToArray());

        public IDataObject SelectedPathsDataObject => SelectedPaths.ToFileDropList();

        public IList<FileViewModel> SelectedItems => Navigation.SelectedItems;

        public FileViewModel SelectedItem => Navigation.SelectedItem;

        public string SelectionText
        {
            get
            {
                var fileCount = Navigation.SelectedItems.Where(item => item.IsFolder == false).Count();
                var folderCount = Navigation.SelectedItems.Where(item => item.IsFolder == true).Count();

                if (fileCount + folderCount == 0) { return ""; }

                var text = "";
                if (fileCount > 0)
                {
                    text += $@"{fileCount} file{(fileCount > 1 ? "s" : "")}{(folderCount > 0 ? " / " : "")}";
                }

                if (folderCount > 0)
                {
                    text += $@"{folderCount} folder{(folderCount > 1 ? "s" : "")}";
                }
                return text;
            }
        }

        public string SelectionStatusText => $"{(string.IsNullOrEmpty(SelectionText) ? "" :  "Selecting ")}{SelectionText}.";
   
        private Settings.FileManager settings;
        public Settings.FileManager Settings 
        { 
            get => settings; 
            set 
            {
                if(settings != null)
                {
                    settings.PropertyChanged -= Settings_PropertyChanged;
                }

                settings = value;

                if(settings != null)
                {
                    settings.PropertyChanged += Settings_PropertyChanged;
                }
                OnPropertyChanged(nameof(Settings));
                Settings_PropertyChanged(this);
            }
        }

        public FileManagerViewModel()
        {
            Navigation.PropertyChanged += Navigation_PropertyChanged;
            Settings = new Settings.FileManager();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e = null)
        {
            if (e == null || e.PropertyName == nameof(Settings.Current))
            {
                Navigation.Goto(Settings.Current);
            }

            if (e == null || e.PropertyName == nameof(Settings.ShowHiddenFiles))
            {
                Navigation.ShowHiddenFiles = Settings.ShowHiddenFiles;
            }
        }

        private void Navigation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SelectedItems))
            {
                OnPropertyChanged(nameof(SelectedItems));
                OnPropertyChanged(nameof(SelectedPaths));
                OnPropertyChanged(nameof(SelectedPathsDataObject));
                OnPropertyChanged(nameof(SelectionStatusText));
            }

            if (e.PropertyName == nameof(SelectedItem))
            {
                OnPropertyChanged(nameof(SelectedItem));
            }

            if(e.PropertyName == nameof(Navigation.FullPath))
            {
                if(Settings.Current != Navigation.FullPath)
                {
                    Settings.Current = Navigation.FullPath;
                }
            }
        }

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            Navigation.Dispose();
        }
        #endregion
    }
}
