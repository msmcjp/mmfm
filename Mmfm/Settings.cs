using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

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
