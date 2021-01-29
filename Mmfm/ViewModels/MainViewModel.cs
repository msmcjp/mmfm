using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Mmfm
{
    public class MainViewModel
    {
        public DualFileManagerViewModel DualFileManager { get; } = new DualFileManagerViewModel();

        public IEnumerable<InputBinding> InputBindings {
            get => commands.Select(c => (InputBinding)c);
        }

        private List<CommandItemViewModel> commands = new List<CommandItemViewModel>();

        private bool CanExecute()
        {
            return DualFileManager.ActiveFileManager.CurrentDirectory.FullPath.Length > 0;
        }

        private void ShowsAddFavorite()
        {
            var path = DualFileManager.ActiveFileManager.CurrentDirectory.FullPath;
            var content = new FavoriteRegisterViewModel(path);
            var dialog = new DialogViewModel { Content = content };
            
            Messenger.Default.Send(dialog);
            if (dialog.Result == true)
            {
                var favorite = FileViewModel.CreateFavorite(path, content.FavoriteName, IconExtractor.Extract(path));
                DualFileManager.First.Favorites.Add(favorite);
                DualFileManager.Second.Favorites.Add(favorite);
            }
        }

        public MainViewModel()
        {
            commands.Add(new CommandItemViewModel("Back to parent", "Backspace", new RelayCommand(() => DualFileManager.ActiveFileManager.BackToParent(), CanExecute)));
            commands.Add(new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => DualFileManager.ActiveFileManager.CopyToClipboard(), CanExecute)));
            commands.Add(new CommandItemViewModel("Paste", "Ctrl+V", new RelayCommand(() => DualFileManager.ActiveFileManager.CopyFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Move to Recycle Bin", "Delete", new RelayCommand(() => DualFileManager.ActiveFileManager.DeleteFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Delete", "Shift+Delete", new RelayCommand(() => DualFileManager.ActiveFileManager.DeleteFiles(false), CanExecute)));
            commands.Add(new CommandItemViewModel("Select All", "Alt+U", new RelayCommand(() => DualFileManager.ActiveFileManager.SelectAll(), CanExecute)));
            commands.Add(new CommandItemViewModel("Deselect All", "Shift+Alt+U", new RelayCommand(() => DualFileManager.ActiveFileManager.DeselectAll(), CanExecute)));
            commands.Add(new CommandItemViewModel("Rename", "F2", new RelayCommand(() => DualFileManager.ActiveFileManager.RenameFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Favorite", "Alt+F", new RelayCommand(() => ShowsAddFavorite(), CanExecute)));

            var commandsToRegister = commands.ToArray();
            commands.Add(new CommandItemViewModel("", "Ctrl+Shift+P", new RelayCommand(() => {
                var content = new CommandPaletteViewModel(commandsToRegister);
                Messenger.Default.Send(new OverlayViewModel(content));
            })));
        }
    }
}
