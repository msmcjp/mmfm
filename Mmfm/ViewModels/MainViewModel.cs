using Mmfm.Commands;
using Mmfm.Plugins;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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

        private ICommandItem commandPallete;

        public Settings Settings
        {
            get;
            private set;
        }

        private (Func<Settings> Pack, Action<Settings> Unpack) packager;

        private IEnumerable<IPluggable<DualFileManagerViewModel>> LoadPlugins()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => typeof(IPluggable<DualFileManagerViewModel>).IsAssignableFrom(t));
            var plugins = types.Select(type => (IPluggable<DualFileManagerViewModel>)Activator.CreateInstance(type)).ToArray();

            foreach(var plugin in plugins)
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

        public MainViewModel()
        {
            var plugins = LoadPlugins();

            packager = (
                Pack: () => new Settings()
                {                    
                    FileManagers = DualFileManager.Settings,
                    Plugins = plugins.Aggregate(new ExpandoObject(), (o, p) =>
                    {
                        if (p.Settings != null)
                        {
                            ((IDictionary<string, object>)o)[p.Name] = p.Settings;
                        }
                        return o;
                    })
                },
                Unpack: (settings) =>
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

            var json = Properties.Settings.Default.Settings_json;
            var defaults = packager.Pack();
            packager.Unpack(Settings = Settings.LoadFromJsonOrDefaults(json, defaults));
   
            commandPallete = CreateCommandPallete(plugins);
        }

        public ICommand SaveSettingsCommand => new RelayCommand(() =>
        {
            Properties.Settings.Default.Settings_json = packager.Pack().Json;
            Properties.Settings.Default.Save();
        });
    }
}
