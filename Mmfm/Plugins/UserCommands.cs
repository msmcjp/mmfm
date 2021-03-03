using Mmfm.Commands;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
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
        public struct CommandDescription
        {
            public string Command
            {
                get;
                set;
            }

            public string Key
            {
                get;
                set;
            }
        }

        public string Name => "Commands";

        public IEnumerable<ICommandItem> Commands => CreateuserCommands();

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

        private IDictionary<string, CommandDescription> settings;
        public object Settings
        {
            get => settings;
            set => settings = (value as IDictionary<string, CommandDescription>);
        }

        public event EventHandler SettingsChanged;

        public void ResetToDefault()
        {
            settings = new Dictionary<string, CommandDescription>();
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
                RedirectStandardOutput = false,
                WorkingDirectory = Host.ActiveFileManager.Navigation.FullPath,
                Arguments = $"/c \"{command}\""
            };

            foreach(var entry in dictionary)
            {
                startInfo.EnvironmentVariables.Add(entry.Key, entry.Value);
            }

            return startInfo;
        }

        private IEnumerable<ICommandItem> CreateuserCommands()
        {
            return settings.Select(x => new CommandItemViewModel(
                x.Key,
                x.Value.Key,
                new RelayCommand(() => Process.Start(CreateProcessStartInfo(x.Value.Command)))
            ));
        }
    }
}
