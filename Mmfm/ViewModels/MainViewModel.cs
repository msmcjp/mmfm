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
using System.Threading.Tasks;
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

        private static readonly string defaultItemGroup = "\U0001f4bb PC";

        public IEnumerable<ICommandItem> Commands => new ICommandItem[]
        {
            new CommandItemViewModel("Show settings", "Ctrl+OemComma", new AsyncRelayCommand(async () => await ShowSettingsAsync())),
        };

        public DualFileManagerViewModel DualFileManager { get; } = new DualFileManagerViewModel();

        public IEnumerable<InputBinding> InputBindings
        {
            get
            {
                if (IsShowingContentDialog)
                {
                    return Enumerable.Empty<InputBinding>();
                }
                return commandPallete.Flatten().Where(c => c.InputBinding != null).Select(c => c.InputBinding);
            }
        }

        private readonly FileSystemWatcher settingsWatcher;

        private readonly DriveInfoMonitor driveInfoMonitor;
        public DriveInfoMonitor DriveInfoMonitor => driveInfoMonitor;

        private readonly IEnumerable<string> resourceNames;
        public IEnumerable<string> ResourceNames => resourceNames;

        private ICommandItem commandPallete;

        public ICommand ShowCommandPalleteCommand => commandPallete.Command;
       
        private readonly (Func<Settings> Defaults, Action<Settings> Apply) settingsFactory;

        private bool isShowingContentDialog;
        public bool IsShowingContentDialog
        {
            get => isShowingContentDialog;
            set
            {
                isShowingContentDialog = value;
                OnPropertyChanged(nameof(IsShowingContentDialog));
                OnPropertyChanged(nameof(IsNotShowingContentDialog));
                OnPropertyChanged(nameof(InputBindings));
            }
        }

        public bool IsNotShowingContentDialog => !IsShowingContentDialog;

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

        private (Func<Settings> Defaults, Action<Settings> Apply) CreateSettingsFactory(IEnumerable<IPluggable<DualFileManagerViewModel>> plugins) =>
        (
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
                    (s, p) => (Settings: s.Value, Plugin: p)
                ).ToArray().ForEach(x => x.Plugin.Settings = x.Settings);
                commandPallete = CreateCommandPallete(plugins);
                OnPropertyChanged(nameof(ShowCommandPalleteCommand));

                var shortcutGenerator = CreateShortcutsGenerator(plugins.Where(p => p.Shortcuts != null).SelectMany(p => p.Shortcuts));
                (updateShortcuts = () => DualFileManager.Roots = shortcutGenerator())();
            }
        );

        private CommandItemViewModel CreateCommandPallete(IEnumerable<IPluggable<DualFileManagerViewModel>> plugins)
        {
            var commands = plugins.SelectMany(plugin => plugin.Commands).ToList().AsReadOnly();
            return new CommandItemViewModel("Show Commands", commands.Concat(Commands), true, "Ctrl+Shift+P");
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

        private DriveInfoMonitor CreateDriveInfoMonitor()
        {
            var monitor = new DriveInfoMonitor();
            monitor.DrivesChanged += DriveInfoMonitor_DrivesChanged;

            return monitor;
        }

        private Func<FolderShortcutViewModel[]> CreateShortcutsGenerator(IEnumerable<FolderShortcutViewModel> extra) => () =>
        {
            var specialFolders = new FolderShortcutViewModel[] {
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Desktop" , defaultItemGroup, IconExtractor.Extract("shell32.dll", 34, true) ),
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Documents", defaultItemGroup, IconExtractor.Extract("shell32.dll", 1, true) ),
                new FolderShortcutViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "My Pictures", defaultItemGroup, IconExtractor.Extract("shell32.dll", 325, true) )
            };

            var drives = DriveInfo.GetDrives().Select(di => new FolderShortcutViewModel(
                    di.Name,
                    $"{di.Name.Trim(Path.DirectorySeparatorChar)} {di.DriveDescription()}",
                    defaultItemGroup,
                    di.DriveIcon()
            ));

            return specialFolders.Concat(drives).Concat(extra).ToArray();
        };
        private Action updateShortcuts;        

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

        private void SaveSettings()
        {
            File.WriteAllText(App.SettingsJsonPath, Settings.Json);
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

        private async Task ShowSettingsAsync()
        {
            var settingsEdit = new SettingsEditViewModel(Settings);
            await Messenger.Default.SendAsync(settingsEdit);

            if(settingsEdit.Result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                Settings = settingsEdit.Settings;
                SaveSettings();
            }
        }

        public MainViewModel()
        {
            var plugins = LoadPlugins();

            settingsFactory = CreateSettingsFactory(plugins);
            settingsWatcher = CreateSettingsFileWatcher();
            driveInfoMonitor = CreateDriveInfoMonitor();
            resourceNames = plugins.Select(p => $"Plugins/{p.GetType().Name}.resources.xaml");
            
            Settings = settingsFactory.Defaults();
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
            var plugin = sender as IPluggable<DualFileManagerViewModel>;
            var settings = Settings.Plugins as IDictionary<string, object>;
            if(settings[plugin.Name] != plugin.Settings)
            {
                settings[plugin.Name] = plugin.Settings;
            }
            Settings_PropertyChanged(this, new PropertyChangedEventArgs(nameof(Plugins)));
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSettings();
            OnPropertyChanged(nameof(InputBindings));
            updateShortcuts();
        }

        private void DriveInfoMonitor_DrivesChanged(object sender, EventArgs e) => updateShortcuts();

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
