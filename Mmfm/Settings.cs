using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mmfm
{
    public class Settings : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
                        new TemplateObjectConverter(defaults.Plugins)
                    }
                }
            );
        }

        public static Settings LoadFromFileOrDefaults(string path, Settings defaults)
        {
            return LoadFromJsonOrDefaults(File.ReadAllText(path), defaults);
        }

        [JsonIgnore]
        public string Json => JsonSerializer.Serialize<Settings>(
            this,
            new JsonSerializerOptions
            {
                WriteIndented = true,
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

        private IEnumerable<FileManager> fileManagers;
        public IEnumerable<FileManager> FileManagers
        {
            get => fileManagers;
            set
            {
                fileManagers = value;
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
            HotKey = "Ctrl+OemSemicolon";
        }
    }
}
