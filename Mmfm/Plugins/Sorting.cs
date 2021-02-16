﻿using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmfm.Plugins
{
    public class Sorting : IPluggable<DualFileManagerViewModel>
    {
        public string Name => "Sorting";
        
        private FoldersViewModel Folders => Host.ActiveFileManager?.Navigation.Folders;
        private FilesViewModel Files => Host.ActiveFileManager?.Navigation.Files;

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Sort", CreateSortCommands()),
        };

        private Func<IEnumerable<ICommandItem>> CreateSortCommands()
        {
            return () =>
            {
                List<ICommandItem> commands = new List<ICommandItem>();
                
                if (Folders != null)
                {
                    commands.Add(new CommandItemViewModel("Folders", SortCommandItems(Folders)));
                }

                if (Files != null)
                {
                    commands.Add(new CommandItemViewModel("Files", SortCommandItems(Files)));
                }
               
                return commands.AsReadOnly();
            };
        }

        private IEnumerable<ICommandItem> SortCommandItems<T>(ItemsViewModel<T> itemsVieModel)
            where T : IHasIsSelected, INotifyPropertyChanged
        {
            var descs = ((IDictionary<string, object>)itemsVieModel.SortDescriptions).Values.Cast<ISortDescription<T>>();
            var commands = descs.Select(desc => new CommandItemViewModel($"by {desc.Name}", new CommandItemViewModel[]
            {
                new CommandItemViewModel("Ascending", null, new RelayCommand(() => {
                    desc.IsDescending = false;
                    itemsVieModel.SortDescription = desc;
                })),
                new CommandItemViewModel("Descending", null, new RelayCommand(() => {
                    desc.IsDescending = true;
                    itemsVieModel.SortDescription = desc;
                })),
            }));

            var none = new CommandItemViewModel("Clear", "", new RelayCommand(() => itemsVieModel.SortDescription = null));
            commands = commands.Concat(new CommandItemViewModel[] { none });

            return commands;
        }

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

        private IDictionary<string, bool> GetSortContexts()
        {
            var sortContexts = new Dictionary<string, bool>();

            ISortDescription<FileViewModel> x;

            if ((x = Host.First.Navigation.Folders.SortDescription) != null)
            {
                sortContexts[$"First.Folders.{x.Name}"] = (bool)x.IsDescending;
            }
            if ((x = Host.First.Navigation.Files.SortDescription) != null)
            {
                sortContexts[$"First.Files.{x.Name}"] = (bool)x.IsDescending;
            }
            if ((x = Host.Second.Navigation.Folders.SortDescription) != null)
            {
                sortContexts[$"Second.Folders.{x.Name}"] = (bool)x.IsDescending;
            }
            if ((x = Host.Second.Navigation.Files.SortDescription) != null)
            {
                sortContexts[$"Second.Files.{x.Name}"] = (bool)x.IsDescending;
            }

            return sortContexts;
        }

        private void ClearSortContexts()
        {
            Host.First.Navigation.Folders.SortDescription =
            Host.First.Navigation.Files.SortDescription =
            Host.Second.Navigation.Folders.SortDescription =
            Host.Second.Navigation.Files.SortDescription = null;
        }

        private void SetSortContexts(IDictionary<string, bool> sortContexts)
        {
            foreach (var entry in sortContexts ?? new Dictionary<string, bool>())
            {
                var keys = entry.Key.Split(".".ToCharArray());
                if (keys.Length != 3)
                {
                    continue;
                }
                FileManagerViewModel x = (keys[0] == "First") ? Host.First : Host.Second;
                ItemsViewModel<FileViewModel> y = (keys[1] == "Folders") ? x.Navigation.Folders : x.Navigation.Files;
                var z = y.SortDescriptionsDictionary[keys[2]];
                if (z.IsDescending != entry.Value)
                {
                    z.IsDescending = entry.Value;
                    y.SortDescription = z;
                }
            }
        }

        public object Settings
        {
            get => GetSortContexts();
            set => SetSortContexts(value as IDictionary<string, bool>);
        }

        public event EventHandler RequestInputBindingsUpdate;

        private void OnRequestInputBIndingsUpdate()
        {
            RequestInputBindingsUpdate?.Invoke(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {
            ClearSortContexts();
        }
    }
}