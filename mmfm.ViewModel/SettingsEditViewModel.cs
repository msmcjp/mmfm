using Mmfm.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mmfm.ViewModel
{
    public class SettingsEditViewModel : ContentDialogViewModel
    { 
        public class KeyBinding
        {
            public string CommandName
            {
                get;
                set;
            }

            private string keyGesture;
            public string KeyGesture
            {
                get => keyGesture;
                set
                {
                    keyGesture = value;
                }
            }
        }

        private Settings settings;
        private IEnumerable<PluginSettingEditViewModel> plugins;
        private IEnumerable<KeyBinding> keyBindings;

        public SettingsEditViewModel(Settings settings)
        {
            this.settings = (Settings)settings.Clone();
            plugins = this.settings.Plugins.Select(p => new PluginSettingEditViewModel(p));
            keyBindings = this.settings.KeyBindings.Select(k => new KeyBinding { CommandName = k.Key, KeyGesture = k.Value }).ToList();
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

        public Settings.Themes Theme
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

        public IEnumerable<string> FontFamilies => Fonts.SystemFontFamilies.Select(f => f.ToString());

        public string FontFamily
        {
            get => settings.FontFamily;
            set
            {
                settings.FontFamily = value;
                OnPropertyChanged(nameof(FontFamily));
            }
        }

        [Range(6, 64, ErrorMessageResourceType=typeof(Properties.Resources), ErrorMessageResourceName = "Settings_FontSizeRange")]
        public int FontSize
        {
            get => settings.FontSize;
            set
            {
                settings.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        public IEnumerable<KeyBinding> KeyBindings => keyBindings;      

        public IEnumerable<PluginSettingEditViewModel> Plugins => plugins;

        public Settings EndEdit()
        {
            settings.KeyBindings = KeyBindings.ToDictionary(k => k.CommandName, k => k.KeyGesture);
            return settings;
        }
    }
}
