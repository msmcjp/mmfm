using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Mmfm.Plugin
{
    public class Controller : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Controller";
       
        private bool CanExecute()
        {
            return CurrentDirectory.FullPath.Length > 0;
        }

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Back to parent", "Backspace", new RelayCommand(() => BackToParent(), CanExecute)),
            new CommandItemViewModel("Select All", "Alt+U", new RelayCommand(() => SelectAll(), CanExecute)),
            new CommandItemViewModel("Deselect All", "Shift+Alt+U", new RelayCommand(() => DeselectAll(), CanExecute)),
            new CommandItemViewModel("Go to Top", "Ctrl+Shift+T", new RelayCommand(() => GotoTop(), CanExecute)),
            new CommandItemViewModel("Quit", "Alt+F4", new RelayCommand(() => Application.Current.Shutdown(), () => true)),
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

        public dynamic Settings
        {
            get;
            set;
        }

        public event EventHandler RequestInputBindingsUpdate;

        private void OnRequestInputBIndingsUpdate()
        {
            RequestInputBindingsUpdate?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {

        }

        public void Plugged()
        {
        }

        private FilesViewModel Files => Host.ActiveFileManager.Files;
        
        private CurrentDirectoryViewModel CurrentDirectory => Host.ActiveFileManager.CurrentDirectory;
        
        private void SelectAll()
        {
            foreach (var item in Files.Items)
            {
                if (item.IsNotAlias)
                {
                    item.IsSelected = true;
                }
            }

            foreach (var item in CurrentDirectory.SubDirectories)
            {
                if (item.IsNotAlias)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void DeselectAll()
        {
            foreach (var item in Files.Items)
            {
                item.IsSelected = false;
            }

            foreach (var item in CurrentDirectory.SubDirectories)
            {
                item.IsSelected = false;
            }
        }

        private void GotoTop()
        {
            CurrentDirectory.Current = null;
        }

        private void BackToParent()
        {
            CurrentDirectory.BackToParent();
        }
    }
}
