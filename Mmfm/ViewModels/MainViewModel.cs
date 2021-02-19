using Mmfm.Commands;
using Mmfm.Plugins;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

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

        private readonly FileSystemWatcher settingsWatcher;

        private readonly ICommandItem commandPallete;

        private Settings settings;
        public Settings Settings
        {
            get => settings;
            private set
            {
                if (settings != null)
                {
                    settings.PropertyChanged -= Settings_PropertyChanged;
                }

                settings = value;
                settingsOp.Apply(settings);

                if (settings != null)
                {
                    settings.PropertyChanged += Settings_PropertyChanged;
                }
                OnPropertyChanged(nameof(Settings));
            }
        }

        private readonly (Func<Settings> Defaults, Action<Settings> Apply) settingsOp;

        private IEnumerable<IPluggable<DualFileManagerViewModel>> LoadPlugins()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => typeof(IPluggable<DualFileManagerViewModel>).IsAssignableFrom(t));
            var plugins = types.Select(type => (IPluggable<DualFileManagerViewModel>)Activator.CreateInstance(type)).ToArray();

            foreach (var plugin in plugins)
            {
                plugin.Host = DualFileManager;
                plugin.ResetToDefault();
                plugin.Messenger = Messenger.Default;
                plugin.RequestInputBindingsUpdate += (s, e) => OnPropertyChanged(nameof(InputBindings));
            }

            return plugins;
        }

        private CommandItemViewModel CreateCommandPallete(IEnumerable<IPluggable<DualFileManagerViewModel>> plugins)
        {
            var commands = plugins.SelectMany(plugin => plugin.Commands).ToList().AsReadOnly();
            return new CommandItemViewModel("Show Commands", commands, "Ctrl+Shift+P");
        }

        private FileSystemWatcher CreateSettingsFileWatcher()
        {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(App.SettingsPath))
            {
                Filter = Path.GetFileName(App.SettingsPath),
                EnableRaisingEvents = true
            };
            watcher.Changed += SettingsWatcher_Changed;
            return watcher;
        }

        public MainViewModel()
        {
            var plugins = LoadPlugins();

            settingsOp = (
                Defaults: () => 
                {
                    return new Settings()
                    {
                        FileManagers = new Settings.FileManager[] {
                            new Settings.FileManager(),
                            new Settings.FileManager()
                        },
                        Plugins = plugins.Aggregate(new ExpandoObject(), (o, p) =>
                        {
                            if (p.Settings != null)
                            {
                                ((IDictionary<string, object>)o)[p.Name] = p.Settings;
                            }
                            return o;
                        }),
                    };
                },
                Apply: (settings) =>
                {
                    DualFileManager.Settings = settings.FileManagers;
                    settings.Plugins.Join(
                        plugins,
                        s => s.Key,
                        p => p.Name,
                        (s, p) => p.Settings = s.Value
                    ).ToArray();
                }
            );
            settingsOp.Apply(settingsOp.Defaults());

            commandPallete = CreateCommandPallete(plugins);
            settingsWatcher = CreateSettingsFileWatcher();
        }

        private void SettingsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            LoadSettings();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            File.WriteAllText(App.SettingsPath, Settings.Json);
        }

        private void LoadSettings()
        {
            Settings = Settings.LoadFromFileOrDefaults(
                App.SettingsPath,
                settingsOp.Defaults()
            );
        }

        private ICommand loadSettingsCommand;
        public ICommand LoadSettingsCommand
        {
            get
            {
                if (loadSettingsCommand == null)
                {
                    loadSettingsCommand = new RelayCommand(() => LoadSettings());
                }
                return loadSettingsCommand;
            }
        }        
    }
}
