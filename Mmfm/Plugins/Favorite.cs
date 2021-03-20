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

namespace Mmfm.Plugins
{
    public class Favorite : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Favorite";

        private readonly string FavoriteItemGroup = "\U0001f496 Favorite";

        private NavigationViewModel Navigation => Host.ActiveFileManager?.Navigation;
     
        public event EventHandler SettingsChanged;

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Favorite", "Alt+F", new AsyncRelayCommand(async () => await AddFavoriteAsync(), CanAddFavorite)),
            new CommandItemViewModel("Unfavorite", "Shift+Alt+F", new AsyncRelayCommand(async () => await RemovefavoriteAsync(), CanRemovefavorite)),
            CreateJumptoFavoriteCommand()
        };

        public Messenger Messenger
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
            get => Favorites;
            set => Favorites = value as ObservableCollection<FolderShortcutViewModel>;
        }

        private ObservableCollection<FolderShortcutViewModel> favorites;
        public ObservableCollection<FolderShortcutViewModel> Favorites
        {
            get => favorites;
            set
            {
                if (favorites != null)
                {
                    favorites.CollectionChanged -= Favorites_CollectionChanged;
                }

                favorites = new ObservableCollection<FolderShortcutViewModel>(
                    value.Select(f => new FolderShortcutViewModel(f.Path, f.Name, FavoriteItemGroup, f.Icon)));

                if (favorites != null)
                {
                    favorites.CollectionChanged += Favorites_CollectionChanged;
                }
                Favorites_CollectionChanged(
                    favorites,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
                );
            }
        }

        public void ResetToDefault()
        {
            Favorites = new ObservableCollection<FolderShortcutViewModel>();
        }

        private bool CanAddFavorite()
        {
            return Navigation?.FullPath.Length > 0 && 
                Favorites?.Where(item => item.Path == Navigation?.FullPath).Count() == 0;
        }

        private async Task AddFavoriteAsync()
        {
            var path = Navigation.FullPath;
            var dialog = new FavoriteRegisterViewModel(path);

            await Messenger?.SendAsync(dialog);
            if (dialog.Result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var favorite = new FolderShortcutViewModel(
                    dialog.FullPath,
                    dialog.FavoriteName,
                    FavoriteItemGroup,
                    IconExtractor.Extract(dialog.FullPath)
                );
                Favorites?.Add(favorite);
            }
        }

        private bool CanRemovefavorite()
        {
            return Favorites?.Where(item => item.Path == Navigation.FullPath).Count() != 0;
        }

        private async Task RemovefavoriteAsync()
        {
            var path = Navigation.FullPath;

            FolderShortcutViewModel favorite = null;
            if ((favorite = Favorites?.SingleOrDefault(f => f.Path == path)) == null)
            {
                await Messenger?.SendAsync(new MessageBoxViewModel
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

            await Messenger?.SendAsync(message);
            if (message.Result == System.Windows.MessageBoxResult.Yes)
            {
                Favorites?.Remove(favorite);
            }
        }

        private ICommandItem CreateJumptoFavoriteCommand()
        {
            var itemsFactory = new Func<ICommandItem, IEnumerable<ICommandItem>>((parent) =>
            {
                return Favorites?.Select((f, i) => new CommandItemViewModel(
                    $"{parent.Name}{CommandItem.PathSeparator}{f.Name}",
                    $"Shift+F{i + 1}",
                    new RelayCommand(() =>
                    {
                        Navigation.Goto(f.Path);
                    })
                ));
            });
            return new CommandItemViewModel("Jump to Favorite", itemsFactory);
        }

        private void Favorites_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var favorites = sender as IEnumerable<FolderShortcutViewModel> ?? Enumerable.Empty<FolderShortcutViewModel>();
            
            Host.Roots = DefaultFolderShortcuts.PC().Concat(favorites).ToArray();
            if(e.Action != NotifyCollectionChangedAction.Reset)
            {
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
