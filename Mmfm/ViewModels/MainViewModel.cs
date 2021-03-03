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
    public class MainViewModel : INotifyPropertyChanged, IDisposable
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

        private ICommandItem commandPallete;     
       
        private readonly (Func<Settings> Defaults, Action<Settings> Apply) settingsFactory;

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
                settingsFactory.Apply(settings);

                if (settings != null)
                {
                    settings.PropertyChanged += Settings_PropertyChanged;
                }
                OnPropertyChanged(nameof(Settings));
                OnPropertyChanged(nameof(InputBindings));
            }
        }

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
                plugin.SettingsChanged += Plugin_SettingsChanged;               
            }

            return plugins;
        }

        private CommandItemViewModel CreateCommandPallete(IEnumerable<IPluggable<DualFileManagerViewModel>> plugins)
        {
            var commands = plugins.SelectMany(plugin => plugin.Commands).ToList().AsReadOnly();
            return new CommandItemViewModel("Show Commands", commands, true, "Ctrl+Shift+P");
        }

        private FileSystemWatcher CreateSettingsFileWatcher()
        {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(App.SettingsJsonPath))
            {
                Filter = Path.GetFileName(App.SettingsJsonPath),
                EnableRaisingEvents = true
            };
            watcher.Changed += SettingsWatcher_Changed;
            return watcher;
        }

        private string ReadSettingsJson()
        {
            if (File.Exists(App.SettingsJsonPath))
            {
                var path = App.SettingsJsonPath;
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return "";
        }

        private void LoadSettings()
        {
            // load only externally edited or at initial
            var jsonText = ReadSettingsJson();
            if (Settings?.Json == jsonText)
            {
                return;
            }            
            Settings = Settings.LoadFromJsonOrDefaults(
                jsonText,
                settingsFactory.Defaults()
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

        public MainViewModel()
        {
            var plugins = LoadPlugins();            

            settingsFactory = (
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
                        (s, p) => (Settings : s.Value, Plugin : p)
                    ).ToArray().ForEach(x => x.Plugin.Settings = x.Settings);
                    commandPallete = CreateCommandPallete(plugins);
                    OnPropertyChanged(nameof(InputBindings));
                }
            );
            settingsFactory.Apply(settingsFactory.Defaults());

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

        private void Plugin_SettingsChanged(object sender, EventArgs e)
        {
            if(Settings == null)
            {
                return;
            }
            var plugin = sender as IPluggable<DualFileManagerViewModel>;
            var pi_settings = (IDictionary<string, object>)Settings.Plugins;

            pi_settings[plugin.Name] = plugin.Settings;            
            Settings_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Plugins)));
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            File.WriteAllText(App.SettingsJsonPath, Settings.Json);
        }

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            DualFileManager.Dispose();
        }
        #endregion
    }
}
