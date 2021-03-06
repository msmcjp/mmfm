﻿using Mmfm.Commands;
using Msmc.Patterns.Collections;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mmfm.Plugins
{
    public class UserCommands : IPluggable<DualFileManagerViewModel>
    {        
        public class CommandDescription
        {
            public string Name
            {
                get;
                set;
            }

            public string Command
            {
                get;
                set;
            }

            public string KeyBinding
            {
                get;
                set;
            }
        }
        
        public string Name => "UserCommands";

        public IEnumerable<ICommandItem> Commands => CreateUserCommands();

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

        private ICollection<CommandDescription> settings;
        public object Settings
        {
            get => settings;
            set => settings = (value as ICollection<CommandDescription>);
        }

        public IEnumerable<FolderShortcutViewModel> Shortcuts => null;

        public event EventHandler SettingsChanged;

        public void ResetToDefault()
        {
            settings = new ObservableCollection<CommandDescription>();
        }

        private ProcessStartInfo CreateProcessStartInfo(string command)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "fullpath", $"\"{Host.ActiveFileManager.Navigation.FullPath}\"" },
                { "selected", $"\"{string.Join(" ", Host.ActiveFileManager.SelectedPaths.Select(p => $"\"{p}\""))}\"" },
            };

            var startInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("ComSpec"))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Host.ActiveFileManager.Navigation.FullPath,
                Arguments = $"/c \"{command}\""
            };

            foreach(var entry in dictionary)
            {
                startInfo.EnvironmentVariables.Add(entry.Key, entry.Value);
            }

            return startInfo;
        }

        private IEnumerable<ICommandItem> CreateUserCommands()
        {
            if (settings.Count() == 0)
            {
                return Enumerable.Empty<ICommandItem>();
            }

            var tree = new TreeNode<string, CommandDescription>("");
            foreach(var setting in settings)
            {
                var path = setting.Name.Split(CommandItem.PathSeparator);
                tree[path] = setting;
            };

            return tree.Composite(
                (path, command) => new CommandItemViewModel(
                    string.Join(CommandItem.PathSeparator, path.Skip(1)),
                    command.KeyBinding,
                    new RelayCommand(() => Process.Start(CreateProcessStartInfo(command.Command)))
                ),
                (path, commands) => new CommandItemViewModel(
                    string.Join(CommandItem.PathSeparator, path.Skip(1)),
                    commands
                )
            ).SubCommands;
        }
    }
}
