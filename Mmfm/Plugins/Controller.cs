using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Mmfm.Plugins
{
    public class Controller : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Controller";

        private bool CanExecute()
        {
            return Navigation.FullPath.Length > 0;
        }   

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Back to parent", "Backspace", new RelayCommand(() => BackToParent(), CanExecute)),
            new CommandItemViewModel("Select All", "Alt+U", new RelayCommand(() => SelectAll(), CanExecute)),
            new CommandItemViewModel("Deselect All", "Shift+Alt+U", new RelayCommand(() => DeselectAll(), CanExecute)),
            new CommandItemViewModel("Go to Top", "Ctrl+Shift+T", new RelayCommand(() => GotoTop(), CanExecute)),
            new CommandItemViewModel("Show hidden files", "Alt+Z", new RelayCommand(() => FileManagerSettings.ShowHiddenFiles = true, () => FileManagerSettings.ShowHiddenFiles == false)),
            new CommandItemViewModel("Hide hidden files", "Alt+Shift+Z", new RelayCommand(() => FileManagerSettings.ShowHiddenFiles = false, () => FileManagerSettings.ShowHiddenFiles == true)),
            new CommandItemViewModel("Quit", "Alt+F4", new RelayCommand(() => Application.Current.Shutdown(), () => true)),
            new CommandItemViewModel("Open settings file", "Ctrl+,", new RelayCommand(() => Process.Start(new ProcessStartInfo(App.SettingsPath) { UseShellExecute = true }))),
        };

        public IMessenger Messenger
        {
            get;
            set;
        }

        public DualFileManagerViewModel Host 
        {
            get;
            set;
        }

        public object Settings
        {
            get;
            set;
        }

        public event EventHandler SettingsChanged;

        private void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {

        }

        private Settings.FileManager FileManagerSettings => Host.ActiveFileManager.Settings;

        private FilesViewModel Files => Host.ActiveFileManager.Navigation.Files;
        
        private NavigationViewModel Navigation => Host.ActiveFileManager.Navigation;
        
        private void SelectAll()
        {
            foreach (var item in Navigation.Items)
            {
                if (item.IsNotAlias)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void DeselectAll()
        {
            foreach (var item in Navigation.Items)
            {
                item.IsSelected = false;
            }
        }

        private void GotoTop()
        {
            Navigation.GotoTop();
        }

        private void BackToParent()
        {
            Navigation.BackToParent();
        }
    }
}
