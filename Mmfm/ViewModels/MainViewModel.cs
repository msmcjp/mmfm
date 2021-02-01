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

        private dynamic settings;

        private IEnumerable<ICommandItem> LoadPlugins()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => typeof(IPluggable<DualFileManagerViewModel>).IsAssignableFrom(t));
            var plugins = types.Select(type => (IPluggable<DualFileManagerViewModel>)Activator.CreateInstance(type)).ToArray();

            settings = new ExpandoObject();

            foreach (var plugin in plugins) 
            {            
                plugin.Host = DualFileManager;
                plugin.Messenger = Messenger.Default;
                plugin.RequestInputBindingsUpdate += (s, e) => OnPropertyChanged("InputBindings");
                plugin.Settings = settings;
                plugin.ResetToDefault();
            }

            var json = Properties.Settings.Default.Settings_json;
            if(string.IsNullOrEmpty(json) == false)
            {
                settings = JsonSerializer.Deserialize<ExpandoObject>(
                    json, 
                    new JsonSerializerOptions { 
                        Converters = { 
                            new TemplateObjectConverter(settings) 
                        }
                    }  
                );
            }

            foreach(var plugin in plugins) 
            {
                plugin.Settings = settings;
                plugin.Plugged();
            }

            return plugins.SelectMany(plugin => plugin.Commands).ToList().AsReadOnly();
        }

        public MainViewModel()
        {
            commandPallete = new CommandItemViewModel("Shows Command Pallette", LoadPlugins(), "Ctrl+Shift+P");
        }

        public ICommand SaveSettingsCommand => new RelayCommand(() =>
        {
            Properties.Settings.Default.Settings_json = JsonSerializer.Serialize<ExpandoObject>(settings);
            Properties.Settings.Default.Save();
        });
    }
}
