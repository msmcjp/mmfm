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
        public object Clone() => LoadFromJsonOrDefaults(Json, this);
        #endregion

        public static Settings LoadFromJsonOrDefaults(string json, Settings defaults)
        {
            if (string.IsNullOrEmpty(json))
            {
                return defaults;
            }

            return JsonSerializer.Deserialize<Settings>(
                json,
                new JsonSerializerOptions
                {
                    Converters = {
                        new TemplateObjectConverter(defaults.Plugins),
                        new JsonStringEnumConverter()
                    }
                }
            );
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

        public Settings()
        {
            HotKey = "";
            Theme = ModernWpf.ApplicationTheme.Light;
            AccentColor = "#FF0080C0";
        }
    }
}
