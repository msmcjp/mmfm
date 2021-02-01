using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mmfm;
using Mmfm.Commands;
using Msmc.Patterns.Messenger;

namespace Mmfm.Plugin
{
    public class Favorite : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Favorite";

        private CurrentDirectoryViewModel CurrentDirectory => Host.ActiveFileManager.CurrentDirectory;

        private ObservableCollection<FolderShortcutViewModel> favorites;
        private ObservableCollection<FolderShortcutViewModel> Favorites
        {
            get => favorites;
            set
            {
                if (favorites != null)
                {
                    favorites.CollectionChanged -= Favorites_CollectionChanged;
                }
                
                favorites = value;
               
                if (favorites != null)
                {
                    favorites.CollectionChanged += Favorites_CollectionChanged;
                }
                
                Host.First.Favorites = Host.Second.Favorites = value;
            }
        }

        public event EventHandler RequestInputBindingsUpdate;

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Favorite", "Alt+F", new RelayCommand(() => AddFavorite(), CanAddFavorite)),
            new CommandItemViewModel("Unfavorite", "Shift+Alt+F", new RelayCommand(() => Removefavorite(), CanRemovefavorite)),
            CreateJumptoFavoriteCommand()
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

        public void ResetToDefault()
        {
            Settings.Favorites = Favorites = new ObservableCollection<FolderShortcutViewModel>();
        }

        public void Plugged()
        {
            Favorites = (ObservableCollection<FolderShortcutViewModel>)Settings.Favorites;
        }

        private bool CanAddFavorite()
        {
            return Host.ActiveFileManager.CurrentDirectory.FullPath.Length > 0;
        }

        private void AddFavorite()
        {
            var path = CurrentDirectory.FullPath;
            var content = new FavoriteRegisterViewModel(path);
            var dialog = new DialogViewModel { Content = content };

            Messenger?.Send(dialog);
            if (dialog.Result == true)
            {
                var favorite = new FolderShortcutViewModel(
                    content.FullPath,
                    content.FavoriteName,
                    "\U0001f496 Favorite",
                    IconExtractor.Extract(content.FullPath)
                );
                Favorites.Add(favorite);
            }
        }

        private bool CanRemovefavorite()
        {
            return Host.ActiveFileManager.CurrentDirectory.FullPath.Length > 0;
        }

        private void Removefavorite()
        {
            var path = CurrentDirectory.FullPath;

            FolderShortcutViewModel favorite = null;
            if ((favorite = Favorites.SingleOrDefault(f => f.Path == path)) == null)
            {
                Messenger?.Send(new MessageBoxViewModel
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

            Messenger?.Send(message);
            if (message.Result == System.Windows.MessageBoxResult.Yes)
            {
                Favorites.Remove(favorite);
            }
        }

        private ICommandItem CreateJumptoFavoriteCommand()
        {
            var itemsFactory = new Func<IEnumerable<ICommandItem>>(() =>
            {
                return Favorites.Select((f, i) => new CommandItemViewModel(
                    f.Name,
                    $"Shift+F{i + 1}",
                    new RelayCommand(() =>
                    {
                        CurrentDirectory.Current = f;
                    })
                ));
            });
            return new CommandItemViewModel("Jump to", itemsFactory);
        }

        private void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RequestInputBindingsUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
}
