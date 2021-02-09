using Msmc.Patterns.Messenger;
using Mmfm.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Text.Json;
using System.Reflection;
using System.Dynamic;
using Mmfm.Plugins;

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
                plugin.RequestInputBindingsUpdate += (s, e) => OnPropertyChanged("InputBindings");
            }
            return plugins;
        }

        private Settings LoadSettings(ExpandoObject defaults)
        {
            var json = Properties.Settings.Default.Settings_json;
            if (string.IsNullOrEmpty(json) == true)
            {
                return new Settings { 
                    FileManagers = DualFileManager.Settings,
                    Plugins = defaults 
                };
            }

            return JsonSerializer.Deserialize<Settings>(
                json,
                new JsonSerializerOptions {
                    Converters = {
                        new TemplateObjectConverter(defaults)
                    }
                }
            );
        }

        private CommandItemViewModel CreateCommandPallete(IEnumerable<IPluggable<DualFileManagerViewModel>> plugins)
        {
            var commands = plugins.SelectMany(plugin => plugin.Commands).ToList().AsReadOnly();
            return new CommandItemViewModel("Show Commands", commands, "Ctrl+Shift+P");
        }

        public MainViewModel()
        {
            var plugins = LoadPlugins();
            Settings = LoadSettings(plugins.Aggregate(new ExpandoObject(), (o, p) => 
            {
                if(p.Settings != null)
                {
                    ((IDictionary<string, object>)o)[p.Name] = p.Settings;
                }
                return o; 
            }));

            DualFileManager.Settings = Settings.FileManagers;
            Settings.Plugins.Join(
                plugins, 
                s => s.Key, 
                p => p.Name, 
                (s, p) => p.Settings = s.Value
            ).ToArray();

            commandPallete = CreateCommandPallete(plugins);
        }

        public ICommand SaveSettingsCommand => new RelayCommand(() =>
        {
            Properties.Settings.Default.Settings_json = JsonSerializer.Serialize<Settings>(
                Settings, 
                new JsonSerializerOptions { 
                    WriteIndented = true,
                }
            );
            Properties.Settings.Default.Save();
        });
    }
}
