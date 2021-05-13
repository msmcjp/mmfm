using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Mmfm
{
    public class Settings : INotifyPropertyChanged, ICloneable
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IClonable
        public object Clone() => LoadFromJsonOrDefaults(Json);
        #endregion

        public static Settings Defaults { get; set; }

        public static Settings LoadFromJsonOrDefaults(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return Defaults;
            }

            var settings = JsonSerializer.Deserialize<Settings>(
                json,
                new JsonSerializerOptions
                {
                    Converters = {
                        new TemplateObjectConverter(Defaults.Plugins),
                        new JsonStringEnumConverter()
                    }
                }
            );
            return settings;
        }

        [JsonIgnore]
        public string Json => JsonSerializer.Serialize<Settings>(
            this,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            }
        );

        public class FileManager : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            private bool showHiddenFiles;
            public bool ShowHiddenFiles
            {
                get => showHiddenFiles;
                set
                {
                    showHiddenFiles = value;
                    OnPropertyChanged(nameof(ShowHiddenFiles));
                }
            }

            private string current;
            public string Current
            {
                get => current;
                set
                {
                    current = value;
                    OnPropertyChanged(nameof(Current));
                }
            }
        }

        private string hotKey;
        public string HotKey
        {
            get => hotKey;
            set
            {
                hotKey = value;
                OnPropertyChanged(nameof(HotKey));
            }
        }

        private ModernWpf.ApplicationTheme theme;
        public ModernWpf.ApplicationTheme Theme
        {
            get => theme;
            set
            {
                theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        private string accentColor;
        public string AccentColor
        {
            get => accentColor;
            set
            {
                accentColor = value;
                OnPropertyChanged(nameof(AccentColor));
            }
        }

        private PropertyChangedEventHandler BubbleUpEventHandler
        {
            get => (s, e) => PropertyChanged?.Invoke(s, e);
        }

        private IEnumerable<FileManager> fileManagers;
        public IEnumerable<FileManager> FileManagers
        {
            get => fileManagers;
            set
            {
                fileManagers?.ForEach(s => {
                    s.PropertyChanged -= BubbleUpEventHandler;
                });

                fileManagers = value;

                fileManagers?.ForEach(s => {
                    s.PropertyChanged += BubbleUpEventHandler;
                });

                OnPropertyChanged(nameof(FileManagers));
            }
        }

        private ExpandoObject plugins;
        public ExpandoObject Plugins
        {
            get => plugins;
            set
            {
                plugins = value;
                OnPropertyChanged(nameof(Plugins));
            }
        }

        private IDictionary<string, string> keyBindings;
        public IDictionary<string, string> KeyBindings
        {
            get => keyBindings;
            set
            {
                keyBindings = Defaults?.KeyBindings ?? new Dictionary<string, string>();
                if (value != null)
                {
                    // merge two dictionaries(overwrite with `value`)
                    keyBindings = keyBindings
                        .Concat(value)
                        .GroupBy((pair) =>  pair.Key, (key, values) => values.Last())
                        .ToDictionary((pair) => pair.Key, (pair) => pair.Value);
                }              
                OnPropertyChanged(nameof(KeyBindings));
            }
        }

        private string fontFamily;
        public string FontFamily
        {
            get => fontFamily;
            set
            {
                fontFamily = value;
                OnPropertyChanged(nameof(FontFamily));
            }
        }

        private int fontSize;
        public int FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        public Settings()
        {
            FontFamily = Defaults?.FontFamily ?? "Segoe UI";
            FontSize = Defaults?.FontSize ?? 14;
            HotKey = Defaults?.HotKey ?? "";
            Theme = Defaults?.Theme ?? ModernWpf.ApplicationTheme.Light;
            AccentColor = Defaults?.AccentColor ?? "#FF0080C0";
            KeyBindings = Defaults != null ? new Dictionary<string, string>(Defaults.KeyBindings) : new Dictionary<string, string>();
        }
    }
}
