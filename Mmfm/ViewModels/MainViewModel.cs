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

        private IEnumerable<ICommandItem> Plug(Plugin.IPluggable<DualFileManagerViewModel> plugin)
        {
            plugin.Host = DualFileManager;
            plugin.Messenger = Messenger.Default;
            plugin.RequestInputBindingsUpdate += RaiseInputBindingsChanged;
            plugin.Plugged();

            return plugin.Commands;
        }

        private IEnumerable<ICommandItem> LoadPlugins()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => typeof(Plugin.IPluggable<DualFileManagerViewModel>).IsAssignableFrom(t));
            var commands = new List<ICommandItem>();
            var plugins = types.Select(type => (Plugin.IPluggable<DualFileManagerViewModel>)Activator.CreateInstance(type));

            return plugins.SelectMany(plugin => Plug(plugin)).ToList().AsReadOnly();
        }

        public MainViewModel()
        {
            commandPallete = new CommandItemViewModel("Shows Command Pallette", LoadPlugins(), "Ctrl+Shift+P");
        }

        private void RaiseInputBindingsChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("InputBindings");
        }
    }
}
