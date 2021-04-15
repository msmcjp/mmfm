using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mmfm
{
    public class SettingsEditViewModel : ContentDialogViewModel
    { 
        private Settings settings;
        private IEnumerable<PluginSettingEditViewModel> plugins;

        public SettingsEditViewModel(Settings settings)
        {
            this.settings = (Settings)settings.Clone();
            plugins = this.settings.Plugins.Select(p => new PluginSettingEditViewModel(p));
        }

        public string HotKey
        {
            get => settings.HotKey;
            set
            {
                settings.HotKey = value;
                OnPropertyChanged(nameof(HotKey));
            }
        }

        public ModernWpf.ApplicationTheme Theme
        {
            get => settings.Theme;
            set
            {
                settings.Theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        public string AccentColor
        {
            get => settings.AccentColor;
            set
            {
                settings.AccentColor = value;
                OnPropertyChanged(nameof(AccentColor));
            }
        }

        public IEnumerable<PluginSettingEditViewModel> Plugins => plugins;

        public Settings Settings => settings;
    }
}
