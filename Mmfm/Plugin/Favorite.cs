using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private ObservableCollection<FolderShortcutViewModel> favorites;

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

        private CurrentDirectoryViewModel CurrentDirectory => Host.ActiveFileManager.CurrentDirectory;

        public void Plugged()
        {
            favorites = new ObservableCollection<FolderShortcutViewModel>();
            Host.First.Favorites = Host.Second.Favorites = favorites;
            favorites.CollectionChanged += Favorites_CollectionChanged;
            LoadFavorites();
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
                favorites.Add(favorite);
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
            if ((favorite = favorites.SingleOrDefault(f => f.Path == path)) == null)
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
                favorites.Remove(favorite);
            }
        }

        private ICommandItem CreateJumptoFavoriteCommand()
        {
            var itemsFactory = new Func<IEnumerable<ICommandItem>>(() =>
            {
                return favorites.Select((f, i) => new CommandItemViewModel(
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

        private void LoadFavorites()
        {
            if (Properties.Settings.Default.Favorites != null)
            {
                foreach (var favorite in Properties.Settings.Default.Favorites)
                {
                    favorites.Add(JsonSerializer.Deserialize<FolderShortcutViewModel>(favorite));
                }
            }
        }

        private void SaveFavorites()
        {
            var stringCollection = new StringCollection();
            foreach (var favorite in favorites)
            {
                stringCollection.Add(JsonSerializer.Serialize(favorite));
            }
            Properties.Settings.Default.Favorites = stringCollection;
            Properties.Settings.Default.Save();
        }

        private void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveFavorites();
            RequestInputBindingsUpdate?.Invoke(this, EventArgs.Empty);
        }
    }
}
