using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Mmfm
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public DualFileManagerViewModel DualFileManager { get; } = new DualFileManagerViewModel();

        public IEnumerable<InputBinding> InputBindings => commandPallete.InputBindings;

        private ICommandItem commandPallete;

        private bool CanExecute()
        {
            return DualFileManager.ActiveFileManager.CurrentDirectory.FullPath.Length > 0;
        }

        private ObservableCollection<FileViewModel> favorites = new ObservableCollection<FileViewModel>();

        private void Favorite()
        {
            var path = DualFileManager.ActiveFileManager.CurrentDirectory.FullPath;
            var content = new FavoriteRegisterViewModel(path);
            var dialog = new DialogViewModel { Content = content };
            
            Messenger.Default.Send(dialog);
            if (dialog.Result == true)
            {
                var favorite = FileViewModel.CreateFavorite(path, content.FavoriteName, IconExtractor.Extract(path));
                favorites.Add(favorite);
            }
        }

        private void Unfavorite()
        {
            var path = DualFileManager.ActiveFileManager.CurrentDirectory.FullPath;

            FileViewModel favorite = null;
            if((favorite = favorites.SingleOrDefault(f => f.Path == path)) == null){
                Messenger.Default.Send(new MessageBoxViewModel
                {
                    Caption = "Error",
                    Text = $"{path} is not registered to favorite.",
                    Icon = System.Windows.MessageBoxImage.Error,
                    Button = System.Windows.MessageBoxButton.OK
                });
                return;
            }

            var message = new MessageBoxViewModel
            {
                Caption = "Confirm",
                Text = $"Remove {path} from favorite?",
                Icon = System.Windows.MessageBoxImage.Question,
                Button = System.Windows.MessageBoxButton.YesNo
            };

            Messenger.Default.Send(message);
            if (message.Result == System.Windows.MessageBoxResult.Yes)
            {
                favorites.Remove(favorite);
            }
        }

        private ICommandItem CreateJumptoFavoriteCommand(IList<FileViewModel> favorites)
        {
            var itemsFactory = new Func<IEnumerable<ICommandItem>>(() =>
            {
                return favorites.Select((f, i) => new CommandItemViewModel(
                    f.Name,
                    $"Shift+F{i + 1}",
                    new RelayCommand(() =>
                    {
                        DualFileManager.ActiveFileManager.JumpTo(f);
                    })
                ));
            });
            return new CommandItemViewModel("Jump to", itemsFactory);
        }

        public MainViewModel()
        {
            var commands = new List<ICommandItem>();
            commands.Add(new CommandItemViewModel("Back to parent", "Backspace", new RelayCommand(() => DualFileManager.ActiveFileManager.BackToParent(), CanExecute)));
            commands.Add(new CommandItemViewModel("Copy", "Ctrl+C", new RelayCommand(() => DualFileManager.ActiveFileManager.CopyToClipboard(), CanExecute)));
            commands.Add(new CommandItemViewModel("Paste", "Ctrl+V", new RelayCommand(() => DualFileManager.ActiveFileManager.CopyFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Move to Recycle Bin", "Delete", new RelayCommand(() => DualFileManager.ActiveFileManager.DeleteFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Delete", "Shift+Delete", new RelayCommand(() => DualFileManager.ActiveFileManager.DeleteFiles(false), CanExecute)));
            commands.Add(new CommandItemViewModel("Select All", "Alt+U", new RelayCommand(() => DualFileManager.ActiveFileManager.SelectAll(), CanExecute)));
            commands.Add(new CommandItemViewModel("Deselect All", "Shift+Alt+U", new RelayCommand(() => DualFileManager.ActiveFileManager.DeselectAll(), CanExecute)));
            commands.Add(new CommandItemViewModel("Rename", "F2", new RelayCommand(() => DualFileManager.ActiveFileManager.RenameFiles(), CanExecute)));
            commands.Add(new CommandItemViewModel("Favorite", "Alt+F", new RelayCommand(() => Favorite(), CanExecute)));
            commands.Add(new CommandItemViewModel("Unfavorite", "Shift+Alt+F", new RelayCommand(() => Unfavorite(), CanExecute)));           
            commands.Add(CreateJumptoFavoriteCommand(favorites));
            
            commandPallete = new CommandItemViewModel("", commands, "Ctrl+Shift+P");

            favorites.CollectionChanged += (s, e) => OnPropertyChanged("InputBindings");
            DualFileManager.First.Favorites = DualFileManager.Second.Favorites = favorites;
        } 
    }
}
