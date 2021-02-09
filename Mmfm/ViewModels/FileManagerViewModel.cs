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

        public IList<FileViewModel> SelectedItems => Navigation.SelectedItems;

        public FileViewModel SelectedItem => Navigation.SelectedItem;

        public string SelectionStatusText
        {
            get
            {
                var fc = Navigation.SelectedItems.Where(item => item.IsFolder == false).Count();
                var dc = Navigation.SelectedItems.Where(item => item.IsFolder == true).Count();

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
            Navigation.Roots = DefaultFolderShortcuts.PC();
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
                OnPropertyChanged(nameof(SelectionStatusText));
            }

            if (e.PropertyName == nameof(SelectedItem))
            {
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }
}
